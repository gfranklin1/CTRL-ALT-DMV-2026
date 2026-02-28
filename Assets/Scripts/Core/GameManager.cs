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

// Central state machine for the game. Only exists in level scenes (not HQ).
// Scripts fall back to GameState.Playing when Instance is null.
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
        // Clear so other scripts don't call into a destroyed object.
        // Without this, the stale Instance keeps its last GameState (e.g. Fail),
        // which blocks player movement when returning to the HQ scene.
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
