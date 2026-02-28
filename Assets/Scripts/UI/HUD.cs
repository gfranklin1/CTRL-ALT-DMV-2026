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

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
        OnStateChanged(GameManager.Instance?.CurrentState ?? GameState.MissionBrief);

        MissionData mission = MissionManager.Instance?.CurrentMission;
        if (mission != null && objectiveText != null)
            objectiveText.text = $"OBJECTIVE: Photograph {mission.targetAction} — ${mission.payoutAmount} payout";

        if (flashOverlay != null) flashOverlay.alpha = 0f;
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        if (viewfinderBorder != null)
            viewfinderBorder.SetActive(state == GameState.CameraRaised);
        if (crosshair != null)
            crosshair.SetActive(state == GameState.Playing || state == GameState.Escaping);
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
