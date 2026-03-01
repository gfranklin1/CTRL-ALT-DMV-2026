using UnityEngine;

[CreateAssetMenu(fileName = "Celebrity", menuName = "Paparazzi/Celebrity")]
public class CelebrityData : ScriptableObject
{
    public string displayName = "Celebrity";
    [TextArea] public string bio = "";
}
