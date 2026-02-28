using UnityEngine;

// Keeps this object's face aligned with the main camera each frame (billboard effect).
public class BillboardFaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        transform.forward = cam.transform.forward;
    }
}
