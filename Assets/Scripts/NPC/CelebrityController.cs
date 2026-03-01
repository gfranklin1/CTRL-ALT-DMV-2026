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

    [Header("Identity")]
    [SerializeField] public string displayName = "Celebrity";
    [SerializeField] public CelebrityAction targetAction = CelebrityAction.WavingAtFan;
    [SerializeField] public int payoutAmount = 500;

    [Header("Visuals")]
    [SerializeField] bool debugColors = true;
    [SerializeField] Renderer rend;
    [SerializeField] Color normalColor = Color.yellow;
    [SerializeField] Color suspiciousColor = Color.red;
    [SerializeField] Color fleeingColor = new Color(0.6f, 0f, 0f);
    [SerializeField] GameObject actionIndicator;

    NavMeshAgent agent;
    CelebState state = CelebState.Patrolling;
    CelebrityAction currentAction = CelebrityAction.None;
    Transform player;
    Vector3 baseScale;
    float scaleTimer;

    public CelebState State => state;
    public CelebrityAction CurrentAction => currentAction;

    // Keep for backwards compat if anything calls it, but targetAction is now Inspector-set
    public void SetTargetAction(CelebrityAction action) => targetAction = action;

    public void Initialize(CelebrityDefinition def, WaypointPath path)
    {
        displayName  = def.displayName;
        targetAction = def.targetAction;
        payoutAmount = def.payoutAmount;
        waypointPath = path;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        baseScale = transform.localScale;
        if (actionIndicator != null) actionIndicator.SetActive(false);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (debugColors && rend != null) rend.material.color = normalColor;
        TransitionToState(CelebState.Patrolling);
    }

    // Switches the celebrity to a new behavior state.
    // Stops all running coroutines first to prevent overlapping routines.
    void TransitionToState(CelebState newState)
    {
        StopAllCoroutines();
        state = newState;
        if (actionIndicator != null)
            actionIndicator.SetActive(newState == CelebState.PerformingAction);

        switch (newState)
        {
            case CelebState.Patrolling:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                if (debugColors && rend != null) rend.material.color = normalColor;
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
                if (debugColors && rend != null) rend.material.color = suspiciousColor;
                break;

            case CelebState.Fleeing:
                agent.isStopped = true;
                currentAction = CelebrityAction.None;
                transform.localScale = baseScale;
                if (debugColors && rend != null) rend.material.color = fleeingColor;
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
        currentAction = targetAction;
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
        // Keep indicator at a fixed world-space offset so it is unaffected by scale pulse
        if (actionIndicator != null && actionIndicator.activeSelf)
            actionIndicator.transform.position = transform.position + new Vector3(0f, 2.2f, 0f);

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
