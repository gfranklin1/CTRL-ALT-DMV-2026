using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotoResultUI : MonoBehaviour
{
    public static PhotoResultUI Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Text gradeText;
    [SerializeField] Text scoreText;
    [SerializeField] Text payoutText;
    [SerializeField] Text actionMatchText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public void Show(PhotoResult result)
    {
        RunData.LastResult = result;
        if (panel != null) panel.SetActive(true);
        if (gradeText != null)      gradeText.text      = result.gradeLabel;
        if (scoreText != null)      scoreText.text       = $"Score: {result.totalScore:F0} / 100";
        if (payoutText != null)     payoutText.text      = $"Payout: ${result.payout}";
        if (actionMatchText != null)
            actionMatchText.text = result.targetActionMatch ? "✓ ACTION CAPTURED!" : "✗ Wrong moment...";

        StopAllCoroutines();
        StartCoroutine(AutoAdvance());
    }

    IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(2f);
        if (panel != null) panel.SetActive(false);
        GameManager.Instance?.TransitionTo(GameState.Escaping);
    }
}
