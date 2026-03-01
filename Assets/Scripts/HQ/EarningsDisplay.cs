using UnityEngine;
using UnityEngine.UI;

public class EarningsDisplay : MonoBehaviour
{
    [SerializeField] Text label;
    [SerializeField] Text repText;   // optional — wire in editor

    void Start()
    {
        RunData.Load();
        if (label   != null) label.text   = $"TOTAL EARNINGS: ${RunData.TotalEarnings}";
        if (repText != null) repText.text = $"REP: {RunData.RepTierName}  ({RunData.Reputation}/100)";
    }
}
