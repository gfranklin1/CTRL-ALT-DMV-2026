using UnityEngine;

public class MissionTimer : MonoBehaviour
{
    float timeRemaining;
    bool running;

    void Start()
    {
        timeRemaining = MissionManager.Instance?.CurrentMission?.missionTime ?? 120f;
        GameManager.OnStateChanged += OnStateChanged;
        OnStateChanged(GameManager.Instance?.CurrentState ?? GameState.MissionBrief);
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        running = state == GameState.Playing ||
                  state == GameState.CameraRaised ||
                  state == GameState.PhotoTaken ||
                  state == GameState.Escaping;
    }

    void Update()
    {
        if (!running) return;

        timeRemaining -= Time.deltaTime;
        HUD.Instance?.SetTimer(Mathf.Max(0f, timeRemaining));

        if (timeRemaining <= 0f)
        {
            running = false;
            GameManager.Instance?.TransitionTo(GameState.Fail);
        }
    }
}
