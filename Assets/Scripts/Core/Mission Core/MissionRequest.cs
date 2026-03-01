using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionRequest", menuName = "Scriptable Objects/MissionRequest")]

public class MissionRequest : ScriptableObject
{
    //Selected from common names list
    public string employer;

    //Selected from implemented job types
    public string jobType;

    //Selected from implemented maps list
    public string location;

    //Randomly generated based on multipliers from risklevel and modifiers
    public int reward;

    //Risk level generated based on job difficulty
    public float riskLevel;

    //The name of the celebrity who we need to photograph
    public string targetName;

    //The type of modifier for added difficulty. NONE for no added modifier
    public string modifier;

    public void missionInit (string employer, string jobType, string location, int reward, float riskLevel, string targetName, string modifier)
    {
        this.employer = employer;
        this.jobType = jobType;
        this.location = location;
        this.reward = reward;
        this.riskLevel = riskLevel;
        this.targetName = targetName;
        this.modifier = modifier;
    }

}
