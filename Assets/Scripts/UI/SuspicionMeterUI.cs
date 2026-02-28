using UnityEngine;
using UnityEngine.UI;

public class SuspicionMeterUI : MonoBehaviour
{
    [SerializeField] Image fillBar;
    [SerializeField] float pulseThreshold = 0.75f;
    [SerializeField] float pulseSpeed = 8f;
    [SerializeField] float pulseAmount = 0.06f;

    [Header("Colors")]
    [SerializeField] Color safeColor    = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] Color warningColor = new Color(1f, 0.6f, 0f);
    [SerializeField] Color dangerColor  = new Color(0.9f, 0.1f, 0.1f);

    void Update()
    {
        if (SuspicionSystem.Instance == null || fillBar == null) return;

        float s = SuspicionSystem.Instance.Suspicion;
        fillBar.fillAmount = s;

        // Dynamic color
        if (s < 0.5f)
            fillBar.color = Color.Lerp(safeColor, warningColor, s / 0.5f);
        else
            fillBar.color = Color.Lerp(warningColor, dangerColor, (s - 0.5f) / 0.5f);

        // Pulse at high suspicion
        if (s > pulseThreshold)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            fillBar.transform.localScale = new Vector3(1f, pulse, 1f);
        }
        else
        {
            fillBar.transform.localScale = Vector3.one;
        }
    }
}
