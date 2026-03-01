using System.Collections.Generic;

public static class RunData
{
    public static int TotalEarnings;
    public static PhotoResult LastResult;
    public static string LastMissionTitle;

    public static List<PhotoResult> SessionResults = new List<PhotoResult>();

    // Index into PinboardData.Entries where this session's photos begin.
    // Used to remove session photos if the player gets caught.
    public static int SessionPhotoStartIndex;

    // Returns the best payout per celebrity — repeated photos of the same
    // celebrity only count once (highest-scoring shot wins).
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
            return total;
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
}
