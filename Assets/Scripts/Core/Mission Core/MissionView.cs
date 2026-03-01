using UnityEngine;
using UnityEngine.UIElements;

public class MissionView : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset listEntryTemplate;

    MissionListEntryHandler missionListEntryHandler;
    bool missionListInitialized;

    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        // Initialize the mission list controller only once to avoid duplicate event subscriptions
        if (missionListEntryHandler == null)
        {
            missionListEntryHandler = new MissionListEntryHandler();
        }

        if (!missionListInitialized)
        {
            missionListEntryHandler.InitializeMissionList(uiDocument.rootVisualElement, listEntryTemplate);
            missionListInitialized = true;
        }
    }
}
