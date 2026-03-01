using UnityEngine;

public class SuspicionSystem : MonoBehaviour
{
    public static SuspicionSystem Instance { get; private set; }

    [SerializeField] float decayRateCameraDown = 0.1f;
    [SerializeField] float decayRateCameraRaised = 0.05f;

    float suspicion;
    bool isBusted;

    CelebrityController _celeb;
    CelebrityController celeb => _celeb != null ? _celeb : (_celeb = FindFirstObjectByType<CelebrityController>());
    PlayerController player;
    CrowdNPC[] crowd;

    public float Suspicion => suspicion;
    public static event System.Action OnBusted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        crowd = FindObjectsByType<CrowdNPC>(FindObjectsSortMode.None);
    }

    // Instantly add a fixed amount of suspicion (e.g. failed bribe penalty).
    public void AddFlatSuspicion(float amount)
    {
        if (isBusted) return;
        suspicion = Mathf.Clamp01(suspicion + amount);
    }

    // Called by DetectionZone each frame when detection conditions are met.
    // ratePerSecond is the base accumulation rate (1.0 = 1 per second).
    public void AddSuspicion(float ratePerSecond)
    {
        if (isBusted) return;

        float mult = 1f;

        // Celebrity state multiplier
        if (celeb != null && celeb.State == CelebState.Suspicious)
            mult *= 2.5f;

        // Player movement multipliers
        if (player != null)
        {
            if (player.IsCrouching) mult *= 0.5f;
            if (player.IsSprinting) mult *= 1.8f;

            // Distance to celebrity
            if (celeb != null)
            {
                float dist = Vector3.Distance(player.transform.position, celeb.transform.position);
                if (dist < 3f)       mult *= 2.0f;
                else if (dist > 10f) mult *= 0.4f;
            }

            // Crowd cover: reduce suspicion when blending in
            int nearbyNPCs = 0;
            foreach (var npc in crowd)
            {
                if (npc != null && Vector3.Distance(player.transform.position, npc.transform.position) <= 2f)
                    nearbyNPCs++;
            }
            if (nearbyNPCs >= 2) mult *= 0.6f;
        }

        suspicion = Mathf.Clamp01(suspicion + ratePerSecond * mult * Time.deltaTime);
    }

    void Update()
    {
        if (isBusted) return;

        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;

        // Passive decay
        if (state == GameState.Playing)
            suspicion = Mathf.Clamp01(suspicion - decayRateCameraDown * Time.deltaTime);
        else if (state == GameState.CameraRaised)
            suspicion = Mathf.Clamp01(suspicion - decayRateCameraRaised * Time.deltaTime);

        // Bust check
        if (suspicion >= 1f)
        {
            isBusted = true;
            celeb?.TriggerFlee();
            OnBusted?.Invoke();
        }
    }
}
