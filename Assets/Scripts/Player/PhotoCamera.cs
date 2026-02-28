using UnityEngine;
using System.Collections;

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
        RunData.LastResult = result;

        // Capture camera view to texture (no UI overlay)
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

        PinboardData.Add(tex, new PinboardEntry
        {
            grade = result.gradeLabel,
            payout = result.payout,
            missionTitle = RunData.LastMissionTitle ?? "",
            rotation = Random.Range(-15f, 15f)
        });
        Destroy(tex);

        PhotoResultUI.Instance?.Show(result);
        HUD.Instance?.ShowFlash();

        GameManager.Instance?.TransitionTo(GameState.PhotoTaken);
        StartCoroutine(FinishPhoto());
    }

    // Waits 2 seconds after taking a photo, then transitions to Escaping.
    // The state check prevents double-transitions if PhotoResultUI already advanced.
    IEnumerator FinishPhoto()
    {
        yield return new WaitForSeconds(2f);
        if (GameManager.Instance?.CurrentState == GameState.PhotoTaken)
            GameManager.Instance?.TransitionTo(GameState.Escaping);
    }
}
