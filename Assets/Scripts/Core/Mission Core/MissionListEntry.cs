using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MissionListEntry
{
    // This function retrieves a reference to the 
    // character name label inside the UI element.
    GroupBox detailsGroup;
    Label employerNameLabel;
    Label jobTypeLabel;
    Label locationLabel;
    Label rewardLabel;
    Label riskLevelLabel;
    Label targetNameLabel;
    Label modifierLabel;
    public void SetButtonElement(VisualElement visualElement)
    {
        //Map vars to UXML UI elements
        detailsGroup = visualElement.Q<GroupBox>("details-group");
        employerNameLabel = detailsGroup.Q<Label>("employer-name");
        jobTypeLabel = detailsGroup.Q<Label>("job-type");
        locationLabel = detailsGroup.Q<Label>("target-name");
        rewardLabel = detailsGroup.Q<Label>("location");
        riskLevelLabel = detailsGroup.Q<Label>("reward");
        targetNameLabel = detailsGroup.Q<Label>("risk-level");
        modifierLabel = detailsGroup.Q<Label>("modifier-name");

    }

    public void SetMissionData(MissionRequest missionData)
    {
        employerNameLabel.text = "Employer: " + missionData.employer;
        jobTypeLabel.text = "Job Desc: " + missionData.jobType;
        locationLabel.text = "Place: " + missionData.location;
        rewardLabel.text = "Reward: " + missionData.reward + "";
        riskLevelLabel.text = "Risk: " + missionData.riskLevel + "";
        targetNameLabel.text = "Target: " + missionData.targetName;
        modifierLabel.text = "Modifier: " + missionData.modifier;

    }
}
