using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyguardSpawner : MonoBehaviour
{
    [SerializeField] GameObject bodyguardPrefab;
    [SerializeField] Transform[] spawnPoints;   // candidate positions for stationary guards

    // Yield one frame so CelebritySpawner.Start() finishes first
    IEnumerator Start()
    {
        yield return null;

        var mission = MissionManager.Instance?.CurrentMission;
        if (mission?.bodyguards == null || mission.bodyguards.Length == 0) yield break;

        // Shuffle spawn-point pool so stationary guards land in different spots each load
        var pool = new List<Transform>(spawnPoints);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        int poolIdx = 0;

        var spawnedCelebs = FindObjectsByType<CelebrityController>(FindObjectsSortMode.None);

        foreach (var def in mission.bodyguards)
        {
            if (def.guardType == BodyguardType.Stationary)
            {
                if (poolIdx >= pool.Count) continue;
                var pt = pool[poolIdx++];
                var go = Instantiate(bodyguardPrefab, pt.position, pt.rotation);
                go.GetComponent<BodyguardController>()?.Initialize(def);

                var spawnPt = pt.GetComponent<BodyguardSpawnPoint>();
                if (spawnPt != null)
                {
                    bool illegal = IsInIllegalZone(pt.position);
                    go.GetComponent<BodyguardBribeHandler>()
                      ?.InitializeBribe(spawnPt.bribeCost, spawnPt.bribeSuccessChance,
                                        spawnPt.bribeDisableDuration, illegal);
                }
            }
            else // Following
            {
                CelebrityController target = def.followsCelebrityIndex < spawnedCelebs.Length
                    ? spawnedCelebs[def.followsCelebrityIndex] : null;
                Vector3 pos = target != null
                    ? target.transform.position + target.transform.right * 1.8f
                    : Vector3.zero;
                var go = Instantiate(bodyguardPrefab, pos, Quaternion.identity);
                go.GetComponent<BodyguardController>()?.Initialize(def, target);
                go.GetComponent<BodyguardBribeHandler>()
                  ?.InitializeBribe(def.bribeCost, def.bribeSuccessChance,
                                    def.bribeDisableDuration, def.isIllegalZone);
            }
        }
    }

    bool IsInIllegalZone(Vector3 pos)
    {
        var hits = Physics.OverlapSphere(pos, 0.5f, ~0, QueryTriggerInteraction.Collide);
        foreach (var h in hits)
            if (h.CompareTag("IllegalZone")) return true;
        return false;
    }
}
