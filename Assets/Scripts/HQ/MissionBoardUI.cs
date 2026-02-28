using UnityEngine;
using UnityEngine.UI;

public class MissionBoardUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Transform cardContainer;
    [SerializeField] GameObject missionCardPrefab;
    [SerializeField] Text earningsText;
    [SerializeField] Button closeButton;

    void Start()
    {
        MissionBoard.OnBoardOpened += Show;
        MissionBoard.OnBoardClosed += Hide;

        if (panel != null)
            panel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    void OnDestroy()
    {
        MissionBoard.OnBoardOpened -= Show;
        MissionBoard.OnBoardClosed -= Hide;
    }

    void Show(MissionData[] missions)
    {
        if (panel != null)
            panel.SetActive(true);

        UpdateEarnings();
        PopulateCards(missions);
    }

    void UpdateEarnings()
    {
        if (earningsText != null)
            earningsText.text = $"TOTAL EARNINGS: ${RunData.TotalEarnings}";
    }

    void PopulateCards(MissionData[] missions)
    {
        if (cardContainer == null || missionCardPrefab == null) return;

        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        foreach (MissionData mission in missions)
        {
            GameObject card = Instantiate(missionCardPrefab, cardContainer);
            MissionCard mc = card.GetComponent<MissionCard>();
            if (mc != null)
                mc.Init(mission);
        }
    }

    void OnCloseClicked()
    {
        MissionBoard.Instance?.Close();
    }

    void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}
