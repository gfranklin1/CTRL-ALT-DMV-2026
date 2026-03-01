using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MissionListEntryHandler
{
    // UXML template for list entries
    VisualTreeAsset m_ListEntryTemplate;

    // UI element references
    ListView missionList;
    GroupBox detailsGroup;
    Label jobTypeLabel;
    Label locationLabel;
    Label rewardLabel;
    Label targetNameLabel;
    Label modifierLabel;

    List<MissionRequest> allMissions;

    public void InitializeMissionList(VisualElement root, VisualTreeAsset listElementTemplate)
    {
        if (root == null)
        {
            Debug.LogError("InitializeMissionList: root is null");
            return;
        }
        if (listElementTemplate == null)
        {
            Debug.LogError("InitializeMissionList: listElementTemplate is null (assign it in MissionView inspector)");
            return;
        }

        EnumerateAllMissions();

        // Store a reference to the template for the list entries
        m_ListEntryTemplate = listElementTemplate;

        // Store a reference to the character list element
        missionList = root.Q<ListView>("mission-list");

        // Store references to the selected character info elements
        jobTypeLabel = root.Q<Label>("job-type");
        targetNameLabel   = root.Q<Label>("target-name");
        locationLabel     = root.Q<Label>("location");
        rewardLabel       = root.Q<Label>("reward");
        modifierLabel     = root.Q<Label>("modifier-name");

        FillMissionList();

        // Register to get a callback when an item is selected
        missionList.selectionChanged += OnMissionSelected;
    }

    //Loads all the missions into a list
    void EnumerateAllMissions()
    {
        allMissions = new List<MissionRequest>();

        // Prefer live-generated missions from the manager; fall back to saved Resources assets
        if (MissionRequestManager.Instance != null)
            allMissions.AddRange(MissionRequestManager.Instance.GetMissions());

        if (allMissions.Count == 0)
            allMissions.AddRange(Resources.LoadAll<MissionRequest>("Missions"));
    }

    void FillMissionList()
    {
        missionList.makeItem = () =>
        {
            // Instantiate the UXML template for the entry
            var newListEntry = m_ListEntryTemplate.Instantiate();

            // Instantiate a controller for the data
            var newListEntryLogic = new MissionListEntry();

            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;

            // Initialize the controller script
            newListEntryLogic.SetButtonElement(newListEntry);

            // Return the root of the instantiated visual tree
            return newListEntry;
        };

        // Set up bind function for a specific list entry
        missionList.bindItem = (item, index) =>
        {
            (item.userData as MissionListEntry)?.SetMissionData(allMissions[index]);
        };

        // Set a fixed item height matching the height of the item provided in makeItem. 
        // For dynamic height, see the virtualizationMethod property.
        missionList.fixedItemHeight = 45;

        // Set the actual item's source list/array
        missionList.itemsSource = allMissions;
    }

    void OnMissionSelected(IEnumerable<object> selectedItems)
    {
        // Get the currently selected item directly from the ListView
        var selectedMission = missionList.selectedItem as MissionRequest;

        // Handle none-selection (Escape to deselect everything)
        if (selectedMission == null)
        {
            if (jobTypeLabel    != null) jobTypeLabel.text    = "";
            if (targetNameLabel != null) targetNameLabel.text = "";
            if (locationLabel   != null) locationLabel.text   = "";
            if (rewardLabel     != null) rewardLabel.text     = "";
            if (modifierLabel   != null) modifierLabel.text   = "";
            return;
        }

        // Fill in mission details (labels may be null if not present in root UXML)
        if (jobTypeLabel    != null) jobTypeLabel.text    = selectedMission.jobType;
        if (targetNameLabel != null) targetNameLabel.text = (selectedMission.celebrities?.Length ?? 1).ToString();
        if (locationLabel   != null) locationLabel.text   = selectedMission.levelSceneName;
        if (rewardLabel     != null) rewardLabel.text     = "$" + selectedMission.payoutAmount;
        if (modifierLabel   != null) modifierLabel.text   = selectedMission.modifier.ToString();
    }
}
