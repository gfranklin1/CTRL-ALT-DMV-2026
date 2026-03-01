using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MissionRequestManager : MonoBehaviour
{
    public static MissionRequestManager Instance { get; private set; }

    [Header("Mission Pool")]
    public string[] npcNames;
    public GameObject[] npcCelebrityObjects;
    public string[] jobTypes;
    // Each entry should match a scene name in Build Settings
    public string[] locations;

    // The mission the player clicked Deploy on — persists into the level scene
    public MissionRequest SelectedMission { get; private set; }

    readonly List<MissionRequest> activeMissions = new List<MissionRequest>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        GenerateXMissions(5);
    }

    // Returns the current active mission list for the board UI
    public MissionRequest[] GetMissions() => activeMissions.ToArray();

    // Called by MissionCard when the player hits Deploy
    public void SelectMission(MissionRequest mr) => SelectedMission = mr;

    public void MissionComplete(MissionRequest mr)
    {
        RunData.AddPayout(mr.payoutAmount);
        activeMissions.Remove(mr);
    }

    public MissionRequest[] GenerateXMissions(int number)
    {
        // Clean up old in-memory instances before creating new ones
        foreach (var m in activeMissions)
            if (m != null) Destroy(m);
        activeMissions.Clear();

#if UNITY_EDITOR
        ClearResourcesFolder();
#endif

        for (int i = 0; i < number; i++)
        {
            var mission = GenerateMission();
            activeMissions.Add(mission);
#if UNITY_EDITOR
            SaveToResources(mission, i);
#endif
        }

        return activeMissions.ToArray();
    }

    public MissionRequest GenerateMission()
    {
        if (npcNames.Length == 0 || jobTypes.Length == 0 || locations.Length == 0)
        {
            Debug.LogWarning("[MissionRequestManager] Arrays not populated in Inspector.");
            return null;
        }

        string mEmployer = npcNames[Random.Range(0, npcNames.Length)];
        string mJobType  = jobTypes[Random.Range(0, jobTypes.Length)];
        string mLocation = locations[Random.Range(0, locations.Length)];

        // Pick a random modifier (including None)
        var modValues  = (MissionModifier[])System.Enum.GetValues(typeof(MissionModifier));
        MissionModifier mModifier = modValues[Random.Range(0, modValues.Length)];

        // Pull display name from CelebrityController if available, fall back to npcNames
        string mTargetName = npcNames[Random.Range(0, npcNames.Length)];
        if (npcCelebrityObjects.Length > 0)
        {
            var obj  = npcCelebrityObjects[Random.Range(0, npcCelebrityObjects.Length)];
            var ctrl = obj != null ? obj.GetComponent<CelebrityController>() : null;
            mTargetName = ctrl != null ? ctrl.displayName : (obj != null ? obj.name : mTargetName);
        }

        // Base risk determined by modifier
        float mRisk = mModifier switch
        {
            MissionModifier.DoubleGuards        => 0.7f,
            MissionModifier.NoBribes            => 0.6f,
            MissionModifier.MultipleCelebrities => 0.5f,
            MissionModifier.Suspicious          => 0.5f,
            MissionModifier.TimeLimit           => 0.5f,
            _                                   => 0.3f
        };

        // Reward multiplier from modifier (riskier job = more payout)
        float modMult = mModifier switch
        {
            MissionModifier.MultipleCelebrities => 1.5f,
            MissionModifier.DoubleGuards        => 1.4f,
            MissionModifier.TimeLimit           => 1.3f,
            MissionModifier.NoBribes            => 1.2f,
            MissionModifier.Suspicious          => 1.2f,
            _                                   => 1.0f
        };

        int baseReward = Mathf.RoundToInt(Random.Range(100, 500) * (1f + mRisk));
        int mReward    = Mathf.RoundToInt(baseReward * modMult);

        // Pick a random non-None action
        var actionValues   = (CelebrityAction[])System.Enum.GetValues(typeof(CelebrityAction));
        var nonNoneActions = System.Array.FindAll(actionValues, a => a != CelebrityAction.None);
        CelebrityAction mAction = nonNoneActions[Random.Range(0, nonNoneActions.Length)];

        // Build celebrity list — MultipleCelebrities adds a second target
        bool multiCeleb = mModifier == MissionModifier.MultipleCelebrities;
        var celebDefs   = new CelebrityDefinition[multiCeleb ? 2 : 1];
        celebDefs[0]    = new CelebrityDefinition { displayName = mTargetName, targetAction = mAction, payoutAmount = mReward };
        if (multiCeleb)
        {
            string secondName   = npcNames[Random.Range(0, npcNames.Length)];
            CelebrityAction act2 = nonNoneActions[Random.Range(0, nonNoneActions.Length)];
            celebDefs[1] = new CelebrityDefinition { displayName = secondName, targetAction = act2, payoutAmount = Mathf.RoundToInt(mReward * 0.8f) };
        }

        // Build bodyguard list — DoubleGuards doubles count, NoBribes zeroes bribe chance
        int guardCount = Mathf.Max(1, Mathf.RoundToInt(mRisk * 3f));
        if (mModifier == MissionModifier.DoubleGuards) guardCount *= 2;
        bool noBribes  = mModifier == MissionModifier.NoBribes;
        var guardDefs  = new BodyguardDefinition[guardCount];
        for (int i = 0; i < guardCount; i++)
        {
            guardDefs[i] = new BodyguardDefinition();
            if (noBribes) guardDefs[i].bribeSuccessChance = 0f;
        }

        // TimeLimit halves the available time
        float mTime = Mathf.Lerp(180f, 60f, mRisk);
        if (mModifier == MissionModifier.TimeLimit) mTime *= 0.5f;

        string displayTarget = multiCeleb ? $"{mTargetName} + {celebDefs[1].displayName}" : mTargetName;
        string title = $"{mJobType}: {displayTarget}";
        string brief = $"Get a shot of {displayTarget} at {mLocation}. Client: {mEmployer}.";
        if (mModifier != MissionModifier.None) brief += $" Warning: {mModifier}.";

        MissionRequest mission = ScriptableObject.CreateInstance<MissionRequest>();
        mission.name           = title;
        mission.missionTitle   = title;
        mission.briefText      = brief;
        mission.employer       = mEmployer;
        mission.jobType        = mJobType;
        mission.modifier       = mModifier;
        mission.levelSceneName = mLocation;
        mission.payoutAmount   = mReward;
        mission.riskLevel      = mRisk;
        mission.targetName     = displayTarget;
        mission.targetAction   = mAction;
        mission.celebrities    = celebDefs;
        mission.bodyguards     = guardDefs;
        mission.missionTime    = mTime;
        mission.minReputation  = Mathf.RoundToInt(mRisk * 60f);
        mission.sharedSuspicion = true;

        return mission;
    }

#if UNITY_EDITOR
    void ClearResourcesFolder()
    {
        const string folder = "Assets/Resources/Missions";
        if (AssetDatabase.IsValidFolder(folder))
        {
            foreach (var guid in AssetDatabase.FindAssets("t:MissionRequest", new[] { folder }))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
        }
        else
        {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
    }

    void SaveToResources(MissionRequest mission, int index)
    {
        const string folder = "Assets/Resources/Missions";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
        // Clone so we don't save the same in-memory instance; the copy lives on disk
        var asset = Instantiate(mission);
        AssetDatabase.CreateAsset(asset, $"{folder}/Mission_{index}.asset");
        AssetDatabase.SaveAssets();
    }
#endif
}
