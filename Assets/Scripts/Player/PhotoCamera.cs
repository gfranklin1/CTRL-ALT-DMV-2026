using UnityEngine;

public class PhotoCamera : MonoBehaviour
{
    [SerializeField] float normalFOV = 60f;
    [SerializeField] float raisedFOV = 40f;
    [SerializeField] float fovLerpSpeed = 8f;

    Camera cam;
    InputSystem_Actions input;
    PhotoScorer scorer;

    public bool CameraIsRaised { get; private set; }

    void Awake()
    {
        cam = GetComponent<Camera>();
        input = new InputSystem_Actions();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        scorer = FindFirstObjectByType<PhotoScorer>();
    }

    void Update()
    {
        if (cam == null) return;

        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;

        // Raise camera when Interact is held in Playing state
        if (state == GameState.Playing && input.Player.Interact.IsPressed())
        {
            CameraIsRaised = true;
            GameManager.Instance?.TransitionTo(GameState.CameraRaised);
        }
        // Lower camera when Interact released while in CameraRaised state
        else if (state == GameState.CameraRaised && !input.Player.Interact.IsPressed())
        {
            CameraIsRaised = false;
            GameManager.Instance?.TransitionTo(GameState.Playing);
        }

        // Take photo: Attack while camera is raised
        if (CameraIsRaised && state == GameState.CameraRaised && input.Player.Attack.WasPressedThisFrame())
        {
            TakePhoto();
        }

        // FOV lerp for raising/lowering camera
        float targetFOV = CameraIsRaised ? raisedFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
    }

    void TakePhoto()
    {
        CameraIsRaised = false;

        PhotoResult result = scorer != null ? scorer.ScoreShot(cam) : new PhotoResult { gradeLabel = "USELESS" };
        RunData.SessionResults.Add(result);
        RunData.LastResult = result;

        // Render the camera's view into an off-screen texture so we can save the photo
        // without capturing any UI overlays. Steps:
        // 1. Create a temporary RenderTexture and point the camera at it
        // 2. Manually call cam.Render() to draw one frame into that texture
        // 3. Copy the pixel data into a Texture2D (CPU-readable)
        // 4. Restore the camera to normal rendering and release the temp texture
        int w = 512, h = 384;
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();

        // Save the captured photo to disk and add it to the pinboard.
        // Random rotation gives each pinned photo a slightly tilted look.
        PinboardData.Add(tex, new PinboardEntry
        {
            grade = result.gradeLabel,
            payout = result.payout,
            missionTitle = RunData.LastMissionTitle ?? "",
            rotation = Random.Range(-15f, 15f)
        });
        // Texture2D was only needed for saving — destroy it to free memory
        Destroy(tex);

        PhotoResultUI.Instance?.Show(result);
        HUD.Instance?.ShowFlash();

        GameManager.Instance?.TransitionTo(GameState.PhotoTaken);
        // PhotoResultUI.AutoAdvance() will return to Playing after 2s
    }
}
