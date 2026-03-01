using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PinboardData
{
    static List<PinboardEntry> _entries;
    static bool _loaded;

    public static List<PinboardEntry> Entries
    {
        get
        {
            if (!_loaded) Load();
            return _entries;
        }
    }

    public static string SaveDir => Application.persistentDataPath + "/Pinboard/";

    // Saves the photo texture as a PNG and its metadata as JSON to persistent storage.
    // File names are sequential (photo_0, photo_1, etc.) so Load() can find them.
    public static void Add(Texture2D tex, PinboardEntry entry)
    {
        if (!_loaded) Load();

        Directory.CreateDirectory(SaveDir);

        int index = _entries.Count;
        string pngPath = SaveDir + "photo_" + index + ".png";
        string jsonPath = SaveDir + "photo_" + index + ".json";

        byte[] pngBytes = tex.EncodeToPNG();
        File.WriteAllBytes(pngPath, pngBytes);

        entry.texturePath = pngPath;
        File.WriteAllText(jsonPath, JsonUtility.ToJson(entry));

        _entries.Add(entry);
    }

    // Reads all saved photos from disk by iterating photo_0.json, photo_1.json, etc.
    // Stops at the first missing index, so files must be sequential with no gaps.
    public static void Load()
    {
        _loaded = true;
        _entries = new List<PinboardEntry>();

        if (!Directory.Exists(SaveDir)) return;

        int i = 0;
        while (true)
        {
            string jsonPath = SaveDir + "photo_" + i + ".json";
            if (!File.Exists(jsonPath)) break;

            string json = File.ReadAllText(jsonPath);
            PinboardEntry entry = JsonUtility.FromJson<PinboardEntry>(json);
            _entries.Add(entry);
            i++;
        }
    }

    // Overwrites the photo and metadata at [index] in place (no renumbering needed).
    public static void Replace(int index, Texture2D tex, PinboardEntry entry)
    {
        if (!_loaded) Load();
        if (index < 0 || index >= _entries.Count) return;

        string pngPath  = SaveDir + "photo_" + index + ".png";
        string jsonPath = SaveDir + "photo_" + index + ".json";

        entry.texturePath = pngPath;
        File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        File.WriteAllText(jsonPath, JsonUtility.ToJson(entry));

        _entries[index] = entry;
    }

    // Removes all entries from [index] onwards and deletes their files from disk.
    // Used to discard session photos when the player gets caught.
    public static void RemoveFrom(int index)
    {
        if (!_loaded) Load();
        for (int i = index; i < _entries.Count; i++)
        {
            string pngPath = SaveDir + "photo_" + i + ".png";
            string jsonPath = SaveDir + "photo_" + i + ".json";
            if (File.Exists(pngPath))  File.Delete(pngPath);
            if (File.Exists(jsonPath)) File.Delete(jsonPath);
        }
        if (_entries.Count > index)
            _entries.RemoveRange(index, _entries.Count - index);
    }

    public static void Clear()
    {
        if (Directory.Exists(SaveDir))
            Directory.Delete(SaveDir, recursive: true);

        _entries = new List<PinboardEntry>();
        _loaded = true;
    }
}
