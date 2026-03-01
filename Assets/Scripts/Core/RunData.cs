using System.Collections.Generic;

public static class RunData
{
    public static int TotalEarnings;
    public static PhotoResult LastResult;
    public static string LastMissionTitle;

    public static List<PhotoResult> SessionResults = new List<PhotoResult>();

    public static int TotalSessionPayout
    {
        get { int t = 0; foreach (var r in SessionResults) t += r.payout; return t; }
    }

    public static void AddPayout(int amount)
    {
        TotalEarnings += amount;
    }

    public static void ClearSession()
    {
        SessionResults = new List<PhotoResult>();
        LastResult = null;
    }
}
