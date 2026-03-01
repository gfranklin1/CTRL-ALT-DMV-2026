using UnityEngine;

[System.Serializable]
public class BodyguardDefinition
{
    public BodyguardType guardType = BodyguardType.Stationary;
    public int followsCelebrityIndex = 0;
}

public enum CelebrityAction
{
    None,
    WavingAtFan,
    DrinkingCoffee
}

[System.Serializable]
public class CelebrityDefinition
{
    public string displayName = "Celebrity";
    public CelebrityAction targetAction = CelebrityAction.WavingAtFan;
    public int payoutAmount = 500;
}

[CreateAssetMenu(fileName = "MissionData", menuName = "Paparazzi/Mission Data")]
public class MissionData : ScriptableObject
{
    public string missionTitle = "Hot Shot";
    [TextArea] public string briefText = "Get the shot. Don't get caught.";
    public CelebrityAction targetAction = CelebrityAction.WavingAtFan;
    public int payoutAmount = 500;
    public string levelSceneName = "SampleScene";
    public float missionTime = 120f;
    public bool sharedSuspicion = true;
    [Header("Reputation")]
    [Range(0, 100)]
    public int minReputation = 0;
    public CelebrityDefinition[] celebrities;
    public BodyguardDefinition[] bodyguards;
}
