using System.IO;
using UnityEditor;
using UnityEngine;

public static class PinboardEditorTools
{
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
