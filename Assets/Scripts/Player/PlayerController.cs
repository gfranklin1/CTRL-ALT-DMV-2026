using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 7f;
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float standHeight = 2f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 1.2f;

    CharacterController cc;
    InputSystem_Actions input;
    Vector3 velocity;

    public bool IsCrouching { get; private set; }
    public bool IsSprinting { get; private set; }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        input = new InputSystem_Actions();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Update()
    {
        // If no GameManager exists (e.g. HQ scene), default to Playing so movement works
        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;
        bool canMove = state == GameState.Playing || state == GameState.CameraRaised || state == GameState.Escaping;

        if (!canMove)
        {
            // Still apply gravity so the player doesn't float, but ignore all input
            if (cc.isGrounded) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            cc.Move(velocity * Time.deltaTime);
            return;
        }

        // Pin downward velocity to a small value when grounded.
        // Using -2f instead of 0f keeps the CharacterController firmly on the ground
        // so cc.isGrounded stays true on slopes and stairs.
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        IsSprinting = input.Player.Sprint.IsPressed() && !IsCrouching;

        if (input.Player.Crouch.WasPressedThisFrame())
            ToggleCrouch();

        float speed = IsCrouching ? crouchSpeed : (IsSprinting ? sprintSpeed : walkSpeed);
        if (state == GameState.CameraRaised) speed *= 0.5f;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Jump velocity derived from v = sqrt(2 * g * h). Gravity is negative,
        // so we negate it here to get a positive upward velocity.
        if (input.Player.Jump.WasPressedThisFrame() && cc.isGrounded && !IsCrouching)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        // Single Move call: horizontal + vertical combined to avoid double collision jitter
        cc.Move((move * speed + velocity) * Time.deltaTime);
    }

    void ToggleCrouch()
    {
        IsCrouching = !IsCrouching;
        cc.height = IsCrouching ? crouchHeight : standHeight;
        // Re-center the collider so it stays anchored at the feet, not the middle
        cc.center = Vector3.up * (cc.height / 2f);
    }
}
