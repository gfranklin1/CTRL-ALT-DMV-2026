using UnityEngine;
using UnityEngine.InputSystem;

public class MissionBoard : MonoBehaviour
{
    public static MissionBoard Instance { get; private set; }

    public static event System.Action<MissionData[]> OnBoardOpened;

    public static event System.Action OnBoardClosed;

    [SerializeField] MissionData[] missions;
    [SerializeField] GameObject proximityPrompt;
    [SerializeField] float interactRange = 2.5f;

    Transform player;
    CameraController cameraController;
    PlayerController playerController;
    InputSystem_Actions input;
    public bool IsOpen => boardOpen;

    bool boardOpen;
    bool wasInRange;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        input = new InputSystem_Actions();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        if (proximityPrompt != null)
            proximityPrompt.SetActive(false);

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
            cameraController = playerGO.GetComponentInChildren<CameraController>();
            playerController = playerGO.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        // Allow closing the board with Escape key
        if (boardOpen)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
            return;
        }

        if (player == null) return;

        // Use sqrMagnitude instead of Vector3.Distance to avoid an expensive sqrt call
        float sqrDist = (transform.position - player.position).sqrMagnitude;
        bool inRange = sqrDist <= interactRange * interactRange;

        if (inRange != wasInRange)
        {
            if (proximityPrompt != null)
                proximityPrompt.SetActive(inRange);
            wasInRange = inRange;
        }

        if (inRange && input.Player.Interact.WasPressedThisFrame())
            Open();
    }

    void Open()
    {
        boardOpen = true;
        if (proximityPrompt != null)
            proximityPrompt.SetActive(false);

        if (cameraController != null)
            cameraController.enabled = false;

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnBoardOpened?.Invoke(missions);
    }

    public void Close()
    {
        boardOpen = false;

        if (cameraController != null)
            cameraController.enabled = true;

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnBoardClosed?.Invoke();
    }
}
