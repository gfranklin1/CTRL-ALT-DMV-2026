using UnityEngine;
using System.Collections;

public class BodyguardBribeHandler : MonoBehaviour
{
    [SerializeField] GameObject promptCanvas;
    [SerializeField] float interactRange = 2.5f;
    [SerializeField] float bribeCooldown = 30f;

    // Set by BodyguardSpawner via InitializeBribe()
    int   bribeCost;
    float bribeSuccessChance;
    float bribeDisableDuration;
    bool  isIllegalZone;

    GuardDetectionZone detectionZone;
    InputSystem_Actions input;
    Transform player;
    float cooldownRemaining;
    bool alreadyBribed;

    public int   BribeCost          => bribeCost;
    public float BribeSuccessChance => bribeSuccessChance;
    public bool  IsIllegalZone      => isIllegalZone;

    public int   EffectiveCost          => Mathf.RoundToInt(bribeCost * RunData.BribeCostMultiplier);
    public float EffectiveSuccessChance => Mathf.Clamp01(bribeSuccessChance + RunData.BribeSuccessModifier);

    public void InitializeBribe(int cost, float chance, float duration, bool illegal)
    {
        bribeCost            = cost;
        bribeSuccessChance   = chance;
        bribeDisableDuration = duration;
        isIllegalZone        = illegal;
    }

    void Awake()
    {
        input         = new InputSystem_Actions();
        detectionZone = GetComponentInChildren<GuardDetectionZone>();
    }

    void OnEnable()  => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (promptCanvas != null) promptCanvas.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;
        if (cooldownRemaining > 0f) cooldownRemaining -= Time.deltaTime;

        GameState state = GameManager.Instance?.CurrentState ?? GameState.Playing;
        if (state != GameState.Playing) { ShowPrompt(false); return; }

        bool inRange  = Vector3.Distance(transform.position, player.position) <= interactRange;
        bool canBribe = inRange && !alreadyBribed && cooldownRemaining <= 0f;

        ShowPrompt(canBribe);

        if (canBribe && !BribeUI.IsOpen && input.Player.Interact.WasPressedThisFrame())
            BribeUI.Instance?.Show(this);
    }

    void ShowPrompt(bool visible)
    {
        if (promptCanvas != null && promptCanvas.activeSelf != visible)
            promptCanvas.SetActive(visible);
    }

    // Called by BribeUI after player confirms
    public void AttemptBribe()
    {
        RunData.TotalEarnings -= EffectiveCost;
        bool success = Random.value < EffectiveSuccessChance;

        if (success)
        {
            alreadyBribed = true;
            StartCoroutine(DisableDetectionFor(bribeDisableDuration));
            BribeUI.Instance?.Close(BribeResult.Success);
        }
        else if (isIllegalZone)
        {
            BribeUI.Instance?.Close(BribeResult.FailIllegal);
        }
        else
        {
            cooldownRemaining = bribeCooldown;
            SuspicionSystem.Instance?.AddFlatSuspicion(0.3f);
            BribeUI.Instance?.Close(BribeResult.FailLegal);
        }
    }

    IEnumerator DisableDetectionFor(float duration)
    {
        if (detectionZone != null) detectionZone.enabled = false;
        yield return new WaitForSeconds(duration);
        if (detectionZone != null) detectionZone.enabled = true;
    }
}

public enum BribeResult { None, Success, FailLegal, FailIllegal }
