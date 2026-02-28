using UnityEngine;
using UnityEngine.AI;

public class CrowdNPC : MonoBehaviour
{
    [SerializeField] float wanderRadius = 8f;
    [SerializeField] float minWaitTime = 2f;
    [SerializeField] float maxWaitTime = 5f;

    NavMeshAgent agent;
    float waitTimer;

    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Start() => SetNewDestination();

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) SetNewDestination();
        }
    }

    void SetNewDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
        randomPoint.y = transform.position.y;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }
}
