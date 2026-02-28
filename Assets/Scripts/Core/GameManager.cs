using UnityEngine;

public enum GameState
{
    MissionBrief,
    Playing,
    CameraRaised,
    PhotoTaken,
    Escaping,
    Win,
    Fail
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.MissionBrief;

    public static event System.Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void TransitionTo(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        Debug.Log($"[GameManager] State → {newState}");
        OnStateChanged?.Invoke(newState);
    }
}
