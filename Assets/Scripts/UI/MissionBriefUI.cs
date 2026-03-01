using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class MissionBriefUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Text titleText;
    [SerializeField] Text briefText;
    [SerializeField] Text actionText;
    [SerializeField] Text payoutText;
    [SerializeField] Text promptText;

    void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
        OnStateChanged(GameManager.Instance?.CurrentState ?? GameState.MissionBrief);

        MissionData mission = MissionManager.Instance?.CurrentMission;
        if (mission != null)
        {
            if (titleText != null) titleText.text = mission.missionTitle.ToUpper();
            if (briefText != null) briefText.text = mission.briefText;
            if (actionText != null) actionText.text = $"TARGET ACTION: {mission.targetAction}";
            if (payoutText != null) payoutText.text = $"PAYOUT: ${mission.payoutAmount}";
        }

        if (promptText != null) promptText.text = "Press SPACE or E to begin";
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        if (panel != null) panel.SetActive(state == GameState.MissionBrief);
    }

    bool transitioning;

    void Update()
    {
        if (GameManager.Instance?.CurrentState != GameState.MissionBrief) return;

        if (!transitioning && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
        {
            transitioning = true;
            StartCoroutine(BeginNextFrame());
        }
    }

    // Delay by one frame so Jump input registered on the same keypress doesn't carry over
    IEnumerator BeginNextFrame()
    {
        yield return null;
        GameManager.Instance?.TransitionTo(GameState.Playing);
    }
}
