using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Attach to the physical computer GameObject in HomeScene.
// Player walks up and presses E to open. Cursor unlocks so they can click
// normally on the UIDocument mission list.
// - Single-click a row to select/highlight it.
// - Double-click a row OR press E again to deploy the selected mission.
// - Press Escape to close without deploying.
public class ComputerInteraction : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] GameObject proximityPrompt;
    [SerializeField] float interactRange = 4f;

    bool isOpen;
    Camera playerCamera;
    PlayerController playerController;
    CameraController cameraController;

    ListView missionList;
    MissionRequest selectedMission;

    InputSystem_Actions input;

    void Awake()
    {
        input = new InputSystem_Actions();
    }

    void OnEnable()  => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerCamera     = playerGO.GetComponentInChildren<Camera>();
            playerController = playerGO.GetComponent<PlayerController>();
            cameraController = playerGO.GetComponentInChildren<CameraController>();
        }

        Debug.Log($"[CI] Start — playerCamera={playerCamera}, playerController={playerController}, uiDocument={uiDocument}");

        if (proximityPrompt != null) proximityPrompt.SetActive(false);

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            Debug.Log($"[CI] rootVisualElement={root}, childCount={root?.childCount}");
            root?.AddToClassList("hidden");
        }
        else
        {
            Debug.LogWarning("[CI] uiDocument is NULL — assign it in the Inspector!");
        }
    }

    void Update()
    {
        if (isOpen)
        {
            if (input.Player.Interact.WasPressedThisFrame() && selectedMission != null)
            {
                Debug.Log($"[CI] E pressed while open — deploying {selectedMission.missionTitle}");
                Deploy(selectedMission);
                return;
            }
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
            return;
        }

        if (playerCamera == null) return;

        float dist = Vector3.Distance(transform.position, playerCamera.transform.position);
        bool inRange = dist <= interactRange;

        if (proximityPrompt != null) proximityPrompt.SetActive(inRange);

        if (inRange && input.Player.Interact.WasPressedThisFrame())
        {
            Debug.Log($"[CI] E pressed in range (dist={dist:F1}) — opening");
            Open();
        }
    }

    void Open()
    {
        isOpen = true;
        selectedMission = null;

        if (proximityPrompt != null) proximityPrompt.SetActive(false);

        if (playerController != null) playerController.enabled = false;
        if (cameraController != null) cameraController.enabled = false;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible   = true;

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            root?.RemoveFromClassList("hidden");

            missionList = root?.Q<ListView>("mission-list");
            Debug.Log($"[CI] Open — root={root}, missionList={missionList}, itemsSource count={missionList?.itemsSource?.Count ?? -1}");

            if (missionList != null)
            {
                missionList.selectionChanged += OnSelectionChanged;
                missionList.itemsChosen += OnItemsChosen;
                Debug.Log("[CI] Subscribed to selectionChanged and itemsChosen");
            }
            else
            {
                Debug.LogWarning("[CI] Could not find ListView named 'mission-list' in UIDocument!");
            }
        }
    }

    void Close()
    {
        Debug.Log("[CI] Close");
        isOpen = false;
        selectedMission = null;

        if (missionList != null)
        {
            missionList.selectionChanged -= OnSelectionChanged;
            missionList.itemsChosen -= OnItemsChosen;
            missionList.ClearSelection();
            missionList = null;
        }

        if (playerController != null) playerController.enabled = true;
        if (cameraController != null) cameraController.enabled = true;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible   = false;

        if (uiDocument != null)
            uiDocument.rootVisualElement?.AddToClassList("hidden");
    }

    void OnSelectionChanged(IEnumerable<object> items)
    {
        selectedMission = missionList?.selectedItem as MissionRequest;
        Debug.Log($"[CI] OnSelectionChanged — selectedMission={selectedMission?.missionTitle ?? "null"}");
    }

    void OnItemsChosen(IEnumerable<object> items)
    {
        Debug.Log("[CI] OnItemsChosen fired (double-click/Enter)");
        foreach (var item in items)
        {
            Debug.Log($"[CI]   item type={item?.GetType().Name}, value={item}");
            if (item is MissionRequest mission)
            {
                Deploy(mission);
                return;
            }
        }
        Debug.LogWarning("[CI] OnItemsChosen — no MissionRequest found in chosen items");
    }

    void Deploy(MissionRequest mission)
    {
        Debug.Log($"[CI] Deploy — {mission.missionTitle} → scene: {mission.levelSceneName}");
        Debug.Log($"[CI]   SceneLoader.Instance={SceneLoader.Instance}, MissionRequestManager.Instance={MissionRequestManager.Instance}");
        Close();
        RunData.LastMissionTitle = mission.missionTitle;
        MissionRequestManager.Instance?.SelectMission(mission);
        SceneLoader.Instance?.LoadScene(mission.levelSceneName);
    }
}
