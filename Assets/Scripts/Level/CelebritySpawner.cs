using UnityEngine;

public class CelebritySpawner : MonoBehaviour
{
    [SerializeField] GameObject celebrityPrefab;
    [SerializeField] Transform[] spawnPoints;

    void Start()
    {
        var mission = MissionManager.Instance?.CurrentMission;
        if (mission?.celebrities == null) return;

        int count = Mathf.Min(mission.celebrities.Length, spawnPoints.Length);
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(celebrityPrefab, spawnPoints[i].position,
                                 spawnPoints[i].rotation);
            var ctrl = go.GetComponent<CelebrityController>();
            var path = spawnPoints[i].GetComponent<WaypointPath>();
            ctrl.Initialize(mission.celebrities[i], path);
        }
    }
}
