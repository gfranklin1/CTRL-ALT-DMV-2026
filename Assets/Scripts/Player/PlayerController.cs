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
        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;
        bool canMove = state == GameState.Playing || state == GameState.CameraRaised || state == GameState.Escaping;

        if (!canMove)
        {
            // Kill horizontal velocity when not allowed to move
            if (cc.isGrounded) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            cc.Move(velocity * Time.deltaTime);
            return;
        }

        // Ground check before processing input
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        IsSprinting = input.Player.Sprint.IsPressed() && !IsCrouching;

        if (input.Player.Crouch.WasPressedThisFrame())
            ToggleCrouch();

        float speed = IsCrouching ? crouchSpeed : (IsSprinting ? sprintSpeed : walkSpeed);
        if (state == GameState.CameraRaised) speed *= 0.5f;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

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
        cc.center = Vector3.up * (cc.height / 2f);
    }
}
