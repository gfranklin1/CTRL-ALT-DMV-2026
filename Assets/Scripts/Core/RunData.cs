using System.Collections.Generic;
using UnityEngine;

public static class RunData
{
    public static int TotalEarnings;
    public static PhotoResult LastResult;
    public static string LastMissionTitle;
    public static int Reputation = 50;

    public static List<PhotoResult> SessionResults = new List<PhotoResult>();

    // Index into PinboardData.Entries where this session's photos begin.
    // Used to remove session photos if the player gets caught.
    public static int SessionPhotoStartIndex;

    public static string RepTierName => Reputation switch {
        >= 80 => "PARASITE",
        >= 60 => "PAPARAZZI",
        >= 40 => "BUM",
        >= 20 => "CREEP",
        _     => "CHUD"
    };

    public static float PayoutMultiplier        => Mathf.Lerp(0.5f,  1.5f,  Reputation / 100f);
    public static float BribeCostMultiplier     => Mathf.Lerp(2.0f,  0.6f,  Reputation / 100f);
    public static float BribeSuccessModifier    => (Reputation - 50f) / 250f;
    public static float SuspicionRateMultiplier => Mathf.Lerp(0.65f, 1.35f, Reputation / 100f);

    public static void ChangeReputation(int delta) =>
        Reputation = Mathf.Clamp(Reputation + delta, 0, 100);

    public static int RepDeltaForGrade(string grade) => grade switch {
        "MONEY SHOT"  => +5,
        "PUBLISHABLE" => +2,
        "WEAK"        => -1,
        _             => -3   // USELESS
    };

    // Returns the best payout per celebrity — repeated photos of the same
    // celebrity only count once (highest-scoring shot wins).
    // Applies rep-based payout multiplier.
    public static int TotalSessionPayout
    {
        get
        {
            var bestPerCeleb = new Dictionary<string, int>();
            foreach (var r in SessionResults)
            {
                if (r.payout <= 0 || string.IsNullOrEmpty(r.celebName)) continue;
                if (!bestPerCeleb.ContainsKey(r.celebName) || r.payout > bestPerCeleb[r.celebName])
                    bestPerCeleb[r.celebName] = r.payout;
            }
            int total = 0;
            foreach (var v in bestPerCeleb.Values) total += v;
            return Mathf.RoundToInt(total * PayoutMultiplier);
        }
    }

    public static void AddPayout(int amount)
    {
        TotalEarnings += amount;
    }

    // Removes pinboard photos taken during this session from disk and memory.
    public static void ClearSessionPhotos()
    {
        PinboardData.RemoveFrom(SessionPhotoStartIndex);
    }

    public static void ClearSession()
    {
        SessionResults = new List<PhotoResult>();
        LastResult = null;
        SessionPhotoStartIndex = PinboardData.Entries.Count;
    }

    public static void Save()
    {
        PlayerPrefs.SetInt("TotalEarnings", TotalEarnings);
        PlayerPrefs.SetInt("Reputation", Reputation);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        TotalEarnings = PlayerPrefs.GetInt("TotalEarnings", 0);
        Reputation = PlayerPrefs.GetInt("Reputation", 50);
    }

    public static void Reset()
    {
        TotalEarnings = 0;
        Reputation    = 50;
        PlayerPrefs.DeleteKey("TotalEarnings");
        PlayerPrefs.DeleteKey("Reputation");
        PlayerPrefs.Save();
    }
}
