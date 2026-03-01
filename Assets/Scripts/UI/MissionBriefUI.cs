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
            if (mission.targets != null && mission.targets.Length > 0)
            {
                if (actionText != null)
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var t in mission.targets)
                    {
                        string name = t.celebrity?.displayName ?? "Unknown";
                        sb.AppendLine($"  {name}: {t.targetAction}");
                    }
                    actionText.text = $"TARGETS:\n{sb.ToString().TrimEnd()}";
                }
                if (payoutText != null)
                {
                    int total = 0;
                    foreach (var t in mission.targets) total += t.payoutAmount;
                    payoutText.text = $"TOTAL PAYOUT: ${total}";
                }
            }
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
