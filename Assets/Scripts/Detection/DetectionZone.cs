using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    [SerializeField] float detectionRate = 1.0f;
    [SerializeField] float cameraDotThreshold = 0.7f;
    [SerializeField] LayerMask occlusionMask = ~0; // everything by default

    CelebrityController celebrity;
    PhotoCamera photoCamera;
    Transform playerTransform;
    bool playerInZone;
    bool isDetecting;

    void Start()
    {
        celebrity = GetComponentInParent<CelebrityController>();
        photoCamera = FindFirstObjectByType<PhotoCamera>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            playerTransform = null;
            SetDetecting(false);
        }
    }

    void Update()
    {
        bool shouldDetect = CheckDetectionConditions();

        if (shouldDetect != isDetecting)
            SetDetecting(shouldDetect);

        if (shouldDetect)
            SuspicionSystem.Instance?.AddSuspicion(detectionRate);
    }

    bool CheckDetectionConditions()
    {
        if (!playerInZone || playerTransform == null) return false;
        if (photoCamera == null || !photoCamera.CameraIsRaised) return false;

        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;
        if (state != GameState.CameraRaised) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        // Dot product: is the camera aimed toward the celebrity?
        Vector3 camToCeleb = (transform.position - cam.transform.position).normalized;
        float dot = Vector3.Dot(cam.transform.forward, camToCeleb);
        if (dot < cameraDotThreshold) return false;

        // Line-of-sight check
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (Physics.Raycast(transform.position, (playerTransform.position - transform.position).normalized, dist - 0.2f, occlusionMask))
            return false;

        return true;
    }

    void SetDetecting(bool detecting)
    {
        isDetecting = detecting;
        celebrity?.SetSuspicious(detecting);
    }
}
