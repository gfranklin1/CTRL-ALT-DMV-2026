using UnityEngine;
using UnityEngine.UIElements;

public class MissionView : MonoBehaviour
{
    [SerializeField] VisualTreeAsset listEntryTemplate;

    MissionListEntryHandler missionListEntryHandler;

    // Start() runs after ALL Awake() calls in the scene, so MissionRequestManager.Instance
    // is guaranteed to exist and have generated its missions by this point.
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        missionListEntryHandler = new MissionListEntryHandler();
        missionListEntryHandler.InitializeMissionList(uiDocument.rootVisualElement, listEntryTemplate);

        var root = uiDocument.rootVisualElement;
        var dayLabel  = root.Q<Label>("stat-day");
        var monLabel  = root.Q<Label>("stat-money");
        var repLabel  = root.Q<Label>("stat-reputation");

        if (dayLabel  != null) dayLabel.text  = $"Day: {RunData.MissionsCompleted}";
        if (monLabel  != null) monLabel.text  = $"Money: ${RunData.TotalEarnings}";
        if (repLabel  != null) repLabel.text  = $"Rep: {RunData.RepTierName} ({RunData.Reputation})";
    }
}
