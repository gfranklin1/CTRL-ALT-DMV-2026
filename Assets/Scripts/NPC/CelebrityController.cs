using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum CelebState { Patrolling, PerformingAction, Suspicious, Fleeing }

public class CelebrityController : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] WaypointPath waypointPath;
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float waitAtWaypointMin = 1f;
    [SerializeField] float waitAtWaypointMax = 3f;

    [Header("Action")]
    [SerializeField] float actionDurationMin = 3f;
    [SerializeField] float actionDurationMax = 6f;
    [SerializeField] float actionChancePerPatrol = 0.4f;

    [Header("Suspicion")]
    [SerializeField] float suspiciousRotateSpeed = 2f;
    [SerializeField] float fleeingDelay = 1.5f;

    [Header("Visuals")]
    [SerializeField] Renderer rend;
    [SerializeField] Color normalColor = Color.yellow;
    [SerializeField] Color suspiciousColor = Color.red;
    [SerializeField] Color fleeingColor = new Color(0.6f, 0f, 0f);

    NavMeshAgent agent;
    CelebState state = CelebState.Patrolling;
    CelebrityAction currentAction = CelebrityAction.None;
    CelebrityAction targetAction = CelebrityAction.None;
    Transform player;
    Vector3 baseScale;
    float scaleTimer;

    public CelebState State => state;
    public CelebrityAction CurrentAction => currentAction;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        baseScale = transform.localScale;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (rend != null) rend.material.color = normalColor;
        TransitionToState(CelebState.Patrolling);
    }

    public void SetTargetAction(CelebrityAction action) => targetAction = action;

    // Switches the celebrity to a new behavior state.
    // Stops all running coroutines first to prevent overlapping routines.
    void TransitionToState(CelebState newState)
    {
        StopAllCoroutines();
        state = newState;

        switch (newState)
        {
            case CelebState.Patrolling:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                if (rend != null) rend.material.color = normalColor;
                currentAction = CelebrityAction.None;
                transform.localScale = baseScale;
                StartCoroutine(PatrolRoutine());
                break;

            case CelebState.PerformingAction:
                agent.isStopped = true;
                StartCoroutine(PerformActionRoutine());
                break;

            case CelebState.Suspicious:
                agent.isStopped = true;
                if (rend != null) rend.material.color = suspiciousColor;
                break;

            case CelebState.Fleeing:
                agent.isStopped = true;
                currentAction = CelebrityAction.None;
                transform.localScale = baseScale;
                if (rend != null) rend.material.color = fleeingColor;
                StartCoroutine(FleeRoutine());
                break;
        }
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            Transform wp = waypointPath?.GetNext();
            if (wp != null)
            {
                agent.SetDestination(wp.position);
                yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.5f);
            }

            yield return new WaitForSeconds(Random.Range(waitAtWaypointMin, waitAtWaypointMax));

            if (Random.value < actionChancePerPatrol)
            {
                TransitionToState(CelebState.PerformingAction);
                yield break;
            }
        }
    }

    IEnumerator PerformActionRoutine()
    {
        currentAction = targetAction != CelebrityAction.None ? targetAction : CelebrityAction.WavingAtFan;
        float duration = Random.Range(actionDurationMin, actionDurationMax);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            scaleTimer += Time.deltaTime * 3f;
            float pulse = 1f + Mathf.Sin(scaleTimer) * 0.08f;
            transform.localScale = baseScale * pulse;
            yield return null;
        }

        transform.localScale = baseScale;
        currentAction = CelebrityAction.None;
        TransitionToState(CelebState.Patrolling);
    }

    IEnumerator FleeRoutine()
    {
        yield return new WaitForSeconds(fleeingDelay);
        GameManager.Instance?.TransitionTo(GameState.Fail);
    }

    // Called by DetectionZone when the player enters/exits the celebrity's awareness radius
    public void SetSuspicious(bool suspicious)
    {
        if (state == CelebState.Fleeing) return;

        if (suspicious && state != CelebState.Suspicious)
            TransitionToState(CelebState.Suspicious);
        else if (!suspicious && state == CelebState.Suspicious)
            TransitionToState(CelebState.Patrolling);
    }

    public void TriggerFlee()
    {
        if (state == CelebState.Fleeing) return;
        TransitionToState(CelebState.Fleeing);
    }

    void Update()
    {
        // When suspicious, slowly rotate to face the player (gives a "staring you down" effect)
        if (state == CelebState.Suspicious && player != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                Quaternion target = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, suspiciousRotateSpeed * Time.deltaTime);
            }
        }
    }
}
