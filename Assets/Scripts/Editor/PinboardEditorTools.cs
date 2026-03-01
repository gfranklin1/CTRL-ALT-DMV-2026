using System.IO;
using UnityEditor;
using UnityEngine;

public static class PinboardEditorTools
{
    [MenuItem("Tools/Reset Player Stats")]
    static void ResetPlayerStats()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Reset Player Stats",
            "Reset TotalEarnings to $0 and Reputation to 50?\n\nThis deletes the saved PlayerPrefs values.",
            "Reset", "Cancel");

        if (!confirm) return;

        RunData.Reset();
        Debug.Log("[RunData] Player stats reset: Earnings=$0, Reputation=50");
    }

    [MenuItem("Tools/Clear Pinboard")]
    static void ClearPinboard()
    {
        string dir = Application.persistentDataPath + "/Pinboard/";
        if (!Directory.Exists(dir))
        {
            EditorUtility.DisplayDialog("Clear Pinboard", "No pinboard data found.", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "Clear Pinboard",
            $"Delete all photos in:\n{dir}\n\nThis cannot be undone.",
            "Delete", "Cancel");

        if (!confirm) return;

        Directory.Delete(dir, recursive: true);
        Debug.Log("[Pinboard] All photos deleted.");
    }
}
