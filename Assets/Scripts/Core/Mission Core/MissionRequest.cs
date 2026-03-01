using UnityEngine;

public enum MissionModifier
{
    None,
    TimeLimit,
    MultipleCelebrities,
    NoBribes,
    Suspicious,
    DoubleGuards
}

[CreateAssetMenu(fileName = "MissionRequest", menuName = "Paparazzi/Mission Request")]
public class MissionRequest : ScriptableObject
{
    [Header("Display")]
    public string missionTitle;
    [TextArea] public string briefText;

    [Header("Origin")]
    public string employer;
    public string jobType;
    public MissionModifier modifier;

    [Header("Level")]
    public string levelSceneName;
    public float missionTime = 120f;
    public bool sharedSuspicion = true;
    [Range(0, 100)] public int minReputation = 0;

    [Header("Target")]
    public string targetName;
    public CelebrityAction targetAction = CelebrityAction.WavingAtFan;
    public int payoutAmount;
    public float riskLevel;

    [Header("Spawning")]
    public CelebrityDefinition[] celebrities;
    public BodyguardDefinition[] bodyguards;
}
