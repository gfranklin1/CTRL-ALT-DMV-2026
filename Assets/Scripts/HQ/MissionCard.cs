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
    public void Init(MissionRequest data)
    {
        if (titleText != null)  titleText.text = data.missionTitle.ToUpper();
        if (actionText != null) actionText.text = data.targetAction.ToString();
        if (payoutText != null)
        {
            float mult = RunData.PayoutMultiplier;
            int adj = Mathf.RoundToInt(data.payoutAmount * mult);
            string tag = mult >= 1.05f ? $"  (+{(int)((mult-1)*100)}% rep)"
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
                // Store selected mission so level scene can read it from MissionManager
                MissionRequestManager.Instance?.SelectMission(data);
                // Close the board (re-enables controllers) before loading
                MissionBoard.Instance?.Close();
                SceneLoader.Instance.LoadScene(data.levelSceneName);
            });
    }
}
