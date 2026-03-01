using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BribeUI : MonoBehaviour
{
    public static BribeUI Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Text costText;
    [SerializeField] Text chanceText;
    [SerializeField] Text fundsText;
    [SerializeField] Text resultText;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;

    BodyguardBribeHandler currentHandler;
    PlayerController      playerController;
    CameraController      cameraController;
    PhotoCamera           photoCamera;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (panel != null) panel.SetActive(false);
        confirmButton?.onClick.AddListener(OnConfirm);
        cancelButton?.onClick.AddListener(OnCancel);
    }

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        cameraController = FindFirstObjectByType<CameraController>();
        photoCamera      = FindFirstObjectByType<PhotoCamera>();
    }

    public void Show(BodyguardBribeHandler handler)
    {
        currentHandler = handler;
        IsOpen = true;

        bool canAfford = RunData.TotalEarnings >= handler.BribeCost;
        if (costText   != null) costText.text   = $"BRIBE COST:      ${handler.BribeCost}";
        if (chanceText != null) chanceText.text = $"SUCCESS CHANCE:  {handler.BribeSuccessChance * 100f:F0}%";
        if (fundsText  != null) fundsText.text  = $"YOUR FUNDS:      ${RunData.TotalEarnings}";
        if (resultText != null) resultText.text = "";
        if (confirmButton != null) confirmButton.interactable = canAfford;

        if (panel != null) panel.SetActive(true);
        SetInputEnabled(false);
    }

    void OnConfirm()
    {
        if (currentHandler == null) return;
        currentHandler.AttemptBribe();
    }

    void OnCancel() => Close(BribeResult.None);

    // Called by handler (or cancel) with the outcome
    public void Close(BribeResult result)
    {
        if (result == BribeResult.None)
        {
            FinishClose();
            return;
        }

        if (result == BribeResult.FailIllegal)
        {
            StartCoroutine(DelayedFail());
            return;
        }

        if (resultText != null)
            resultText.text = result == BribeResult.Success ? "DEAL STRUCK" : "REJECTED";

        StartCoroutine(CloseAfterDelay(result == BribeResult.Success ? 1.2f : 1.5f));
    }

    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        FinishClose();
    }

    IEnumerator DelayedFail()
    {
        if (resultText != null) resultText.text = "BUSTED!";
        yield return new WaitForSeconds(1.5f);
        FinishClose();
        GameManager.Instance?.TransitionTo(GameState.Fail);
    }

    void FinishClose()
    {
        IsOpen = false;
        currentHandler = null;
        if (panel != null) panel.SetActive(false);
        SetInputEnabled(true);
    }

    void SetInputEnabled(bool enabled)
    {
        if (playerController != null) playerController.enabled = enabled;
        if (cameraController  != null) cameraController.enabled  = enabled;
        if (photoCamera       != null) photoCamera.enabled       = enabled;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !enabled;
    }
}
