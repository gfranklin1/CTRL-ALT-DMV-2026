using UnityEngine;
using UnityEngine.AI;

public enum BodyguardType { Stationary, Following }

public class BodyguardController : MonoBehaviour
{
    [SerializeField] BodyguardType type;
    [SerializeField] float followStopDistance = 1.8f;

    [Header("Visuals")]
    [SerializeField] bool  debugColors   = true;
    [SerializeField] Color normalColor   = new Color(0.3f, 0.3f, 0.8f);   // blue-grey
    [SerializeField] Color detectingColor = new Color(1f, 0.3f, 0f);       // orange-red

    NavMeshAgent agent;
    CelebrityController followTarget;
    Renderer rend;

    public void Initialize(BodyguardDefinition def, CelebrityController target = null)
    {
        type = def.guardType;
        followTarget = target;

        if (type == BodyguardType.Following)
        {
            var zone = GetComponentInChildren<GuardDetectionZone>();
            zone?.SetDirectional(false);
        }
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rend  = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        if (type == BodyguardType.Stationary && agent != null)
            agent.enabled = false;

        if (rend != null) rend.material.color = normalColor;
    }

    public void SetDetecting(bool detecting)
    {
        if (debugColors && rend != null)
            rend.material.color = detecting ? detectingColor : normalColor;
    }

    void Update()
    {
        if (type != BodyguardType.Following || agent == null || followTarget == null) return;
        float dist = Vector3.Distance(transform.position, followTarget.transform.position);
        if (dist > followStopDistance)
            agent.SetDestination(followTarget.transform.position);
        else
            agent.ResetPath();
    }
}
