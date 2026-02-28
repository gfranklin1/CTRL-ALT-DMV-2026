using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float pitchClamp = 80f;
    [SerializeField] Transform playerBody;

    InputSystem_Actions input;
    float pitch;

    void Awake()
    {
        input = new InputSystem_Actions();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;
        bool canLook = state == GameState.Playing || state == GameState.CameraRaised || state == GameState.Escaping;

        bool needsCursor = state == GameState.MissionBrief || state == GameState.Win || state == GameState.Fail;
        Cursor.lockState = needsCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = needsCursor;

        if (!canLook) return;

        Vector2 look = input.Player.Look.ReadValue<Vector2>() * mouseSensitivity;

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * look.x);

        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);
        transform.localEulerAngles = Vector3.right * pitch;
    }
}
