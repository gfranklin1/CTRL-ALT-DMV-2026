using UnityEngine;
using UnityEngine.UI;

public class MissionCard : MonoBehaviour
{
    [SerializeField] Text titleText;
    [SerializeField] Text actionText;
    [SerializeField] Text payoutText;
    [SerializeField] Button deployButton;

    // Called by MissionBoardUI when this card is spawned.
    // Populates the UI text and wires the Deploy button to load the mission's scene.
    public void Init(MissionData data)
    {
        if (titleText != null) titleText.text = data.missionTitle.ToUpper();

        if (actionText != null)
        {
            if (data.targets == null || data.targets.Length == 0)
                actionText.text = "NO TARGETS";
            else if (data.targets.Length <= 2)
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < data.targets.Length; i++)
                {
                    if (i > 0) sb.Append(" + ");
                    sb.Append(data.targets[i].targetAction.ToString().ToUpper());
                }
                actionText.text = sb.ToString();
            }
            else
                actionText.text = $"{data.targets.Length} TARGETS";
        }

        if (payoutText != null)
        {
            float mult  = RunData.PayoutMultiplier;
            int   total = 0;
            if (data.targets != null)
                foreach (var t in data.targets) total += t.payoutAmount;
            int   adj   = Mathf.RoundToInt(total * mult);
            string tag  = mult >= 1.05f ? $"  (+{(int)((mult-1)*100)}% rep)"
                        : mult <= 0.95f ? $"  (-{(int)((1-mult)*100)}% rep)"
                        : "";
            payoutText.text = $"${adj}{tag}";
        }

        if (deployButton != null)
            deployButton.onClick.AddListener(() =>
            {
                if (SceneLoader.Instance == null) return;
                deployButton.interactable = false;
                RunData.LastMissionTitle = data.missionTitle;
                // Close the board (re-enables controllers) before loading
                MissionBoard.Instance?.Close();
                SceneLoader.Instance.LoadScene(data.levelSceneName);
            });
    }
}
