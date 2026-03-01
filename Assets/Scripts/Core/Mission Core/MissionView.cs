using UnityEngine;
using UnityEngine.UIElements;

public class MissionView : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset listEntryTemplate;

    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        // Initialize the character list controller
        var missionListEntryHandler = new MissionListEntryHandler();
        missionListEntryHandler.InitializeMissionList(uiDocument.rootVisualElement, listEntryTemplate);
    }
}
