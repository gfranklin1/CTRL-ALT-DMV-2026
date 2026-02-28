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

    public static void Clear()
    {
        if (Directory.Exists(SaveDir))
            Directory.Delete(SaveDir, recursive: true);

        _entries = new List<PinboardEntry>();
        _loaded = true;
    }
}
