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

    void Show(MissionRequest[] missions)
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

    // Destroys old cards and instantiates fresh ones from the missions array.
    // Each card is a prefab with a MissionCard component that wires up its own UI.
    void PopulateCards(MissionRequest[] missions)
    {
        if (cardContainer == null || missionCardPrefab == null) return;

        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        var available = new System.Collections.Generic.List<MissionRequest>();
        foreach (var m in missions)
            if (RunData.Reputation >= m.minReputation)
                available.Add(m);

        foreach (MissionRequest mission in available)
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
