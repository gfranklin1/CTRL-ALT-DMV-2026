using UnityEngine;
using System;

public class JailSystem : MonoBehaviour
{
    public static JailSystem Instance { get; private set; }
    public int BailAmount { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called from CelebrityController.FleeRoutine
    public void HandleBust()
    {
        bool illegal = IsPlayerInIllegalZone();
        float chance = illegal ? 0.70f : 0.25f;
        ResolveBust(chance);
    }

    // Called from BribeUI.DelayedFail
    public void HandleIllegalBust() => ResolveBust(0.80f);

    void ResolveBust(float jailChance)
    {
        if (UnityEngine.Random.value < jailChance)
        {
            BailAmount = 200 + Mathf.RoundToInt(
                (MissionManager.Instance?.CurrentMission?.riskLevel ?? 0.3f) * 500f);
            RunData.ClearSessionPhotos();
            RunData.ChangeReputation(-10);
            GameManager.Instance?.TransitionTo(GameState.Jailed);
        }
        else
        {
            GameManager.Instance?.TransitionTo(GameState.Fail);
        }
    }

    public void AttemptPayBail(Action<string> onDone)
    {
        RunData.TotalEarnings -= BailAmount;
        bool inDebt = RunData.TotalEarnings < 0;
        if (inDebt) RunData.ChangeReputation(-15);
        RunData.Save();
        string msg = inDebt
            ? $"You're ${-RunData.TotalEarnings} in the hole. Rep took a hit."
            : "Bail paid. Don't let it happen again.";
        onDone?.Invoke(msg);
    }

    public void CallParents(Action<string> onDone)
    {
        if (UnityEngine.Random.value < 0.5f)
        {
            RunData.Save();
            onDone?.Invoke("Your dad came through. Get out of here.");
        }
        else
        {
            RunData.TotalEarnings -= BailAmount;
            bool inDebt = RunData.TotalEarnings < 0;
            if (inDebt) RunData.ChangeReputation(-15);
            RunData.Save();
            string msg = inDebt
                ? $"No answer. You owe ${-RunData.TotalEarnings} now."
                : "No answer. You paid your way out.";
            onDone?.Invoke(msg);
        }
    }

    bool IsPlayerInIllegalZone()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return false;
        var hits = Physics.OverlapSphere(player.transform.position, 1f, ~0, QueryTriggerInteraction.Collide);
        foreach (var h in hits)
            if (h.CompareTag("IllegalZone")) return true;
        return false;
    }
}
