using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MissionListEntry
{
    // This function retrieves a reference to the 
    // character name label inside the UI element.
    GroupBox detailsGroup;
    Label jobTypeLabel;
    Label locationLabel;
    Label rewardLabel;
    Label targetNameLabel;
    Label modifierLabel;
    public void SetButtonElement(VisualElement visualElement)
    {
        detailsGroup  = visualElement.Q<GroupBox>("details-group");
        jobTypeLabel  = detailsGroup.Q<Label>("job-type");
        targetNameLabel = detailsGroup.Q<Label>("target-name");
        locationLabel = detailsGroup.Q<Label>("location");
        rewardLabel   = detailsGroup.Q<Label>("reward");
        modifierLabel = detailsGroup.Q<Label>("modifier-name");
    }

    public void SetMissionData(MissionRequest missionData)
    {
        jobTypeLabel.text    = "Job: " + missionData.jobType;
        targetNameLabel.text = "Targets: " + (missionData.celebrities?.Length ?? 1);
        locationLabel.text   = "Place: " + missionData.levelSceneName;
        rewardLabel.text     = "$" + missionData.payoutAmount + "  |  " + RiskLabel(missionData.riskLevel) + " Risk";
        modifierLabel.text   = "Modifier: " + missionData.modifier;
    }

    static string RiskLabel(float risk) =>
        risk >= 0.9f ? "Extreme" :
        risk >= 0.6f ? "High" :
        risk >= 0.4f ? "Med" : "Low";
}
