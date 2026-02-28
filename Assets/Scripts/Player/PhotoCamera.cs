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

        // FOV lerp
        float targetFOV = CameraIsRaised ? raisedFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
    }

    void TakePhoto()
    {
        CameraIsRaised = false;

        PhotoResult result = scorer != null ? scorer.ScoreShot(cam) : new PhotoResult { gradeLabel = "USELESS" };
        RunData.LastResult = result;
        PhotoResultUI.Instance?.Show(result);
        HUD.Instance?.ShowFlash();

        GameManager.Instance?.TransitionTo(GameState.PhotoTaken);
        StartCoroutine(FinishPhoto());
    }

    IEnumerator FinishPhoto()
    {
        yield return new WaitForSeconds(2f);
        if (GameManager.Instance?.CurrentState == GameState.PhotoTaken)
            GameManager.Instance?.TransitionTo(GameState.Escaping);
    }
}
