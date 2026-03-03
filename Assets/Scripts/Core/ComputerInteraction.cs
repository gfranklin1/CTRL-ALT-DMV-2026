using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ComputerInteraction : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] float interactRange = 3f;
    [SerializeField] float uiTotalPx     = 1080f;  // UXML root height in pixels
    [SerializeField] float uiTitlePx     = 100f;   // title label height
    [SerializeField] float uiRowPx       = 100f;   // each list entry height

    Camera playerCamera;
    InputSystem_Actions input;
    ListView         missionList;
    MissionRequest[] missions;
    int hoveredIndex = -1;

    void Awake() => input = new InputSystem_Actions();
    void OnEnable()  => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            playerCamera = playerGO.GetComponentInChildren<Camera>();

        uiDocument?.rootVisualElement?.RemoveFromClassList("hidden");
    }

    void Update()
    {
        if (playerCamera == null) return;

        // ── lazy fetch missions & list ────────────────────────────────────────
        if (missions == null || missions.Length == 0)
            missions = MissionRequestManager.Instance?.GetMissions();

        if (missionList == null)
            missionList = uiDocument?.rootVisualElement?.Q<ListView>("mission-list");

        // ── proximity check ───────────────────────────────────────────────────
        float dist = Vector3.Distance(transform.position, playerCamera.transform.position);
        if (dist > interactRange) { SetHover(-1); return; }

        // ── deploy on E or left click ─────────────────────────────────────────
        bool deploy = input.Player.Interact.WasPressedThisFrame()
                   || Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        // ── Raycast from camera centre ────────────────────────────────────────
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        bool hitComputer = Physics.Raycast(
            ray.origin, ray.direction,
            out RaycastHit hit, interactRange * 2f,
            Physics.AllLayers, QueryTriggerInteraction.Ignore)
            && hit.transform == transform;

        if (!hitComputer) { SetHover(-1); return; }
        if (missions == null || missions.Length == 0) return;

        // ── Map hit Y → slot index ────────────────────────────────────────────
        Vector3 local   = transform.InverseTransformPoint(hit.point);
        float topLocalY = 0.5f - uiTitlePx / uiTotalPx;
        float botLocalY = 0.5f - (uiTitlePx + missions.Length * uiRowPx) / uiTotalPx;
        float t         = Mathf.Clamp01(Mathf.InverseLerp(topLocalY, botLocalY, local.y));
        int   idx       = Mathf.Min(Mathf.FloorToInt(t * missions.Length), missions.Length - 1);

        SetHover(idx);

        if (deploy && hoveredIndex >= 0)
            Deploy(missions[hoveredIndex]);
    }

    // ── Crosshair dot at screen centre ───────────────────────────────────────
    void OnGUI()
    {
        float dist = playerCamera != null
            ? Vector3.Distance(transform.position, playerCamera.transform.position)
            : float.MaxValue;

        if (dist > interactRange) return;

        float cx = Screen.width  * 0.5f;
        float cy = Screen.height * 0.5f;
        float r  = 4f;
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(cx - r - 1, cy - r - 1, r * 2 + 2, r * 2 + 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(cx - r, cy - r, r * 2, r * 2), Texture2D.whiteTexture);
    }

    void SetHover(int idx)
    {
        if (idx == hoveredIndex) return;
        hoveredIndex = idx;
        if (missionList == null) return;
        if (idx < 0) missionList.ClearSelection();
        else         missionList.selectedIndex = idx;
    }

    void Deploy(MissionRequest mission)
    {
        if (mission == null) return;
        RunData.LastMissionTitle = mission.missionTitle;
        MissionRequestManager.Instance?.SelectMission(mission);
        SceneLoader.Instance?.LoadScene(mission.levelSceneName);
    }
}
