using UnityEngine;
using UnityEngine.UI;

public class MissionCard : MonoBehaviour
{
    [SerializeField] Text titleText;
    [SerializeField] Text actionText;
    [SerializeField] Text payoutText;
    [SerializeField] Button deployButton;

    public void Init(MissionData data)
    {
        if (titleText != null)  titleText.text = data.missionTitle.ToUpper();
        if (actionText != null) actionText.text = data.targetAction.ToString();
        if (payoutText != null) payoutText.text = $"${data.payoutAmount}";

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
