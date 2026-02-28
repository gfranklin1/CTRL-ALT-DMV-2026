public static class RunData
{
    public static int TotalEarnings;
    public static PhotoResult LastResult;
    public static string LastMissionTitle;

    public static void AddPayout(int amount)
    {
        TotalEarnings += amount;
    }
}
