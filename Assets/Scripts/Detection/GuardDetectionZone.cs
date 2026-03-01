using UnityEngine;

public class GuardDetectionZone : MonoBehaviour
{
    [SerializeField] float detectionRate = 1.5f;
    [SerializeField] bool isDirectional = true;   // true = Stationary FOV cone; false = Following camera-aim check
    [SerializeField] float viewAngle = 90f;        // half-cone degrees (Stationary only)
    [SerializeField] float cameraDotThreshold = 0.7f; // (Following — same as DetectionZone)
    [SerializeField] LayerMask occlusionMask = ~0;

    PhotoCamera photoCamera;
    BodyguardController guardController;
    Transform playerTransform;
    bool playerInZone;

    public void SetDirectional(bool value) => isDirectional = value;

    void Start()
    {
        photoCamera      = FindFirstObjectByType<PhotoCamera>();
        guardController  = GetComponentInParent<BodyguardController>();
    }

    void OnDisable()
    {
        guardController?.SetDetecting(false);
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Player"))
        {
            playerInZone = true;
            playerTransform = c.transform;
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("Player"))
        {
            playerInZone = false;
            playerTransform = null;
        }
    }

    void Update()
    {
        bool detecting = CheckDetectionConditions();
        guardController?.SetDetecting(detecting);
        if (detecting)
            SuspicionSystem.Instance?.AddSuspicion(detectionRate);
    }

    bool CheckDetectionConditions()
    {
        if (!playerInZone || playerTransform == null) return false;
        if (photoCamera == null || !photoCamera.CameraIsRaised) return false;
        if ((GameManager.Instance?.CurrentState ?? GameState.Playing) != GameState.CameraRaised) return false;

        if (isDirectional)
        {
            // Player must be inside the guard's forward viewing cone
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f) return false;
        }
        else
        {
            // Camera must be aimed toward this guard (same logic as DetectionZone)
            Camera cam = Camera.main;
            if (cam == null) return false;
            Vector3 camToGuard = (transform.position - cam.transform.position).normalized;
            if (Vector3.Dot(cam.transform.forward, camToGuard) < cameraDotThreshold) return false;
        }

        // Line-of-sight check
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        return !Physics.Raycast(
            transform.position,
            (playerTransform.position - transform.position).normalized,
            dist - 0.2f,
            occlusionMask);
    }
}
