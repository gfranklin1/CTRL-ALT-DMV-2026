using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [SerializeField] MissionData missionData;
    [SerializeField] CelebrityController celebrity;

    public MissionData CurrentMission => missionData;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (celebrity != null && missionData != null)
            celebrity.SetTargetAction(missionData.targetAction);
    }
}
