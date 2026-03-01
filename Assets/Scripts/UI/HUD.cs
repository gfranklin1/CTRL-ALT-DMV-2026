using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    [Header("Elements")]
    [SerializeField] GameObject crosshair;
    [SerializeField] GameObject viewfinderBorder;
    [SerializeField] Text objectiveText;
    [SerializeField] CanvasGroup flashOverlay;
    [SerializeField] Text timerText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
        OnStateChanged(GameManager.Instance?.CurrentState ?? GameState.MissionBrief);

        MissionRequest mission = MissionManager.Instance?.CurrentMission;
        if (mission != null && objectiveText != null)
        {
            if (mission.targets != null && mission.targets.Length > 0)
            {
                int total = 0;
                foreach (var t in mission.targets) total += t.payoutAmount;
                string acts = mission.targets.Length == 1
                    ? mission.targets[0].targetAction.ToString()
                    : $"{mission.targets.Length} targets";
                objectiveText.text = $"OBJECTIVE: {acts} — ${total} payout";
            }
            else
                objectiveText.text = "OBJECTIVE: Get the shot";
        }

        if (flashOverlay != null) flashOverlay.alpha = 0f;
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        if (viewfinderBorder != null)
            viewfinderBorder.SetActive(state == GameState.CameraRaised);
        if (crosshair != null)
            crosshair.SetActive(state == GameState.Playing || state == GameState.Escaping);
        if (timerText != null)
            timerText.gameObject.SetActive(state != GameState.MissionBrief &&
                                           state != GameState.Win &&
                                           state != GameState.Fail);
    }

    public void SetTimer(float seconds)
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        timerText.text = $"{m}:{s:D2}";
        timerText.color = seconds <= 15f ? Color.red : Color.white;
    }

    public void ShowFlash()
    {
        if (flashOverlay != null)
            StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        flashOverlay.alpha = 1f;
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            flashOverlay.alpha = Mathf.Lerp(1f, 0f, t / 0.4f);
            yield return null;
        }
        flashOverlay.alpha = 0f;
    }
}
