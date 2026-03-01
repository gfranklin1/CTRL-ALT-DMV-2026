using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float pitchClamp = 80f;
    [SerializeField] Transform playerBody;

    [Header("Smoothing")]
    [SerializeField, Range(0f, 1f)] float lookSmoothing = 0.5f;

    InputSystem_Actions input;
    float pitch;
    float yaw;
    Vector2 smoothedLook;
    bool skipFirstLookFrame;
    bool prevNeedsCursor;

    void Awake()
    {
        input = new InputSystem_Actions();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        skipFirstLookFrame = true; // ignore the cursor-lock repositioning spike (once only)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialise yaw from the player body so first frame isn't a snap
        if (playerBody != null) yaw = playerBody.eulerAngles.y;
    }

    // Cursor state can stay in Update – it's UI-related and shouldn't wait for LateUpdate
    void Update()
    {
        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;

        bool boardOpen = MissionBoard.Instance != null && MissionBoard.Instance.IsOpen;
        bool needsCursor = boardOpen || state == GameState.MissionBrief || state == GameState.Win || state == GameState.Fail;
        if (needsCursor != prevNeedsCursor)
        {
            Cursor.lockState = needsCursor ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = needsCursor;
            prevNeedsCursor = needsCursor;
        }
    }

    // Camera rotation in LateUpdate so it runs AFTER PlayerController.Update moves the body
    void LateUpdate()
    {
        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;
        bool boardOpen = MissionBoard.Instance != null && MissionBoard.Instance.IsOpen;
        bool canLook = !boardOpen && (state == GameState.Playing || state == GameState.CameraRaised || state == GameState.Escaping);

        if (!canLook) return;

        if (skipFirstLookFrame) { skipFirstLookFrame = false; return; }

        Vector2 rawLook = input.Player.Look.ReadValue<Vector2>() * mouseSensitivity;

        // Lerp between the previous frame's look value and the new raw input.
        // lookSmoothing=0 → t=1 → instant (no smoothing)
        // lookSmoothing=1 → t=0 → maximum smoothing (more input lag)
        float t = 1f - lookSmoothing;
        smoothedLook = Vector2.Lerp(smoothedLook, rawLook, t);

        yaw += smoothedLook.x;
        pitch -= smoothedLook.y;
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

        // Apply rotations via absolute angles – avoids floating-point drift from Rotate()
        if (playerBody != null)
            playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
