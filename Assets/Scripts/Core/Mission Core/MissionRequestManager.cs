using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class MissionRequestManager : MonoBehaviour
{
    //Singleton stuff
    public static MissionRequestManager Instance { get; private set; }

    //List of global static variables for data
    //public static readonly string[] npcNames;
    //public static readonly GameObject[] npcCelebrityObjects;
    //public static readonly GameObject[] npcGuardObjects;
    //public static readonly GameObject[] npcRegularObjects;
    //public static readonly string[] jobTypes;
    //public static readonly string[] locations;
    //public static readonly string[] modifiers;
    public string[] npcNames;
    public GameObject[] npcCelebrityObjects;
    public GameObject[] npcGuardObjects;
    public GameObject[] npcRegularObjects;
    public string[] jobTypes;
    public string[] locations;
    public string[] modifiers;

    //List of data we may use later
    private int totalMoneyEarned = 0;
    //private int reputation = 0;
    private int day = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Create first mission
        generateXMissions(5);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void missionComplete(MissionRequest mr)
    {
        totalMoneyEarned += mr.reward;
    }

    public MissionRequest[] generateXMissions(int number)
    {
        MissionRequest[] missions = new MissionRequest[number];

        for (int i = 0; i < number; i++)
        {
            missions[i] = generateMission();
        }

        return missions;
    }

    public MissionRequest generateMission()
    {
        //Randomly generate mission params
        string mEmployer = npcNames[Random.Range(0, npcNames.Length)];
        string mJobType = jobTypes[Random.Range(0, jobTypes.Length)];

        //This doesnt work probably I think vvv
        string mTargetName = npcCelebrityObjects[Random.Range(0, npcCelebrityObjects.Length)].name;
        
        string mLocation = locations[Random.Range(0, locations.Length)];
        string mModifier = modifiers[Random.Range(0, modifiers.Length)];
        float mRisk = 0;

        //Assign risk to certain modifiers
        switch (mModifier)
        {
            default:
                mRisk = 0;
                break;
        }
        
        //Reward calculation based on difficulty
        int mReward = (int) (Random.Range(100, 300) * mRisk);

        //Create the mission instance and initialize it
        MissionRequest mission = ScriptableObject.CreateInstance<MissionRequest>();
        mission.missionInit(mEmployer, mJobType, mLocation, mReward, mRisk, mTargetName, mModifier);

        return mission;
    }
}
