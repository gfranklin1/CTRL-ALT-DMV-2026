using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WinFailUI : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject failPanel;
    [SerializeField] Text winPayoutText;
    [SerializeField] Button winGoHomeButton;
    [SerializeField] Button failRetryButton;
    [SerializeField] Button failRetreatButton;

    void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
        if (winPanel != null)  winPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);

        if (winGoHomeButton != null)   winGoHomeButton.onClick.AddListener(GoHome);
        if (failRetryButton != null)   failRetryButton.onClick.AddListener(Retry);
        if (failRetreatButton != null) failRetreatButton.onClick.AddListener(Retreat);
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        if (state == GameState.Win)
        {
            int payout = RunData.TotalSessionPayout;
            if (winPayoutText != null)
                winPayoutText.text = $"PAYOUT: ${payout}";
            if (winPanel != null) winPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EventSystem.current?.SetSelectedGameObject(winGoHomeButton?.gameObject);
        }
        else if (state == GameState.Fail)
        {
            RunData.ClearSessionPhotos();
            if (failPanel != null) failPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EventSystem.current?.SetSelectedGameObject(failRetryButton?.gameObject);
        }
    }

    void GoHome()
    {
        // Bank total session payout (all shots taken this run) before returning to HQ
        RunData.AddPayout(RunData.TotalSessionPayout);
        SceneLoader.Instance?.LoadHome();
    }

    void Retreat()
    {
        // Go home without banking any payout (player chose to retreat after failing)
        SceneLoader.Instance?.LoadHome();
    }

    void Retry()
    {
        SceneLoader.Instance?.ReloadCurrent();
    }
}
