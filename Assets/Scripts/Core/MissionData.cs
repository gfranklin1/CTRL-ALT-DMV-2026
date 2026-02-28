using UnityEngine;

public enum CelebrityAction
{
    None,
    WavingAtFan,
    DrinkingCoffee
}

[CreateAssetMenu(fileName = "MissionData", menuName = "Paparazzi/Mission Data")]
public class MissionData : ScriptableObject
{
    public string missionTitle = "Hot Shot";
    [TextArea] public string briefText = "Get the shot. Don't get caught.";
    public CelebrityAction targetAction = CelebrityAction.WavingAtFan;
    public int payoutAmount = 500;
    public string levelSceneName = "SampleScene";
}
