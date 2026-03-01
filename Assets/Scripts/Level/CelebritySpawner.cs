using UnityEngine;

public class CelebritySpawner : MonoBehaviour
{
    [SerializeField] GameObject celebrityPrefab;
    [SerializeField] Transform[] spawnPoints;

    void Start()
    {
        var mission = MissionManager.Instance?.CurrentMission;
        if (mission?.targets == null) return;

        int count = Mathf.Min(mission.targets.Length, spawnPoints.Length);
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(celebrityPrefab, spawnPoints[i].position,
                                 spawnPoints[i].rotation);
            var ctrl = go.GetComponent<CelebrityController>();
            var path = spawnPoints[i].GetComponent<WaypointPath>();
            ctrl.Initialize(mission.targets[i], path);
        }
    }
}
