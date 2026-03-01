using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    // Can be manually assigned in the Inspector for one-off test levels.
    // At runtime, MissionRequestManager.SelectedMission takes priority.
    [SerializeField] MissionRequest missionData;

    public MissionRequest CurrentMission =>
        MissionRequestManager.Instance?.SelectedMission ?? missionData;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
}
