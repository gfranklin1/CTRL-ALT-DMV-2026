using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JailUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Text bailText;
    [SerializeField] Text fundsText;
    [SerializeField] Text resultText;
    [SerializeField] Button payBailButton;
    [SerializeField] Button callParentsButton;

    void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
        if (panel != null) panel.SetActive(false);
        payBailButton?.onClick.AddListener(OnPayBail);
        callParentsButton?.onClick.AddListener(OnCallParents);
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        if (panel == null) return;
        panel.SetActive(state == GameState.Jailed);
        if (state != GameState.Jailed) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int bail = JailSystem.Instance?.BailAmount ?? 500;
        if (bailText   != null) bailText.text   = $"BAIL:       ${bail}";
        if (fundsText  != null) fundsText.text  = $"YOUR FUNDS: ${RunData.TotalEarnings}";
        if (resultText != null) resultText.text = "";
        SetButtons(true);
    }

    void OnPayBail()
    {
        SetButtons(false);
        JailSystem.Instance?.AttemptPayBail(msg => StartCoroutine(ShowResultThenHome(msg)));
    }

    void OnCallParents()
    {
        SetButtons(false);
        if (resultText != null) resultText.text = "Calling...";
        StartCoroutine(ParentsDelay());
    }

    IEnumerator ParentsDelay()
    {
        yield return new WaitForSeconds(1.5f);
        JailSystem.Instance?.CallParents(msg => StartCoroutine(ShowResultThenHome(msg)));
    }

    IEnumerator ShowResultThenHome(string msg)
    {
        if (resultText != null) resultText.text = msg;
        yield return new WaitForSeconds(2f);
        SceneLoader.Instance?.LoadHome();
    }

    void SetButtons(bool on)
    {
        if (payBailButton     != null) payBailButton.interactable     = on;
        if (callParentsButton != null) callParentsButton.interactable = on;
    }
}
