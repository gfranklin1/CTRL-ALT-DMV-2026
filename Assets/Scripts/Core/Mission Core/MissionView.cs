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

        // Load the UI document and get the root visual element
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Query the button by name (assuming the UXML has a button with the name "MyButton")
        var myButton = root.Q<Button>("list-entry");

        // Add a listener to the 'clicked' event
        if (myButton != null)
        {
            myButton.clicked += MyButtonClickAction;
        }

    

    // Initialize the character list controller
    var missionListEntryHandler = new MissionListEntryHandler();
        missionListEntryHandler.InitializeMissionList(uiDocument.rootVisualElement, listEntryTemplate);
    }

    private void MyButtonClickAction()
    {
        Debug.Log("Button was clicked!");
        // Add your custom logic here (e.g., loading a scene, opening a panel, etc.)
    }
}
