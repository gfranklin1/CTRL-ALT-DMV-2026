using UnityEngine;
using UnityEngine.UI;

public class EarningsDisplay : MonoBehaviour
{
    [SerializeField] Text label;

    void Start()
    {
        if (label != null)
            label.text = $"TOTAL EARNINGS: ${RunData.TotalEarnings}";
    }
}
