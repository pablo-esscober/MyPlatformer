using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static string TempPath => Path.Combine(Application.persistentDataPath, "save.tmp");

    public static bool HasSave => File.Exists(SavePath);

    public static void Save(SaveData data)
    {
        if (data == null) return;
        data.version = SaveData.CurrentVersion;
        string json = JsonUtility.ToJson(data, prettyPrint: true);

        File.WriteAllText(TempPath, json);
        if (File.Exists(SavePath))
            File.Replace(TempPath, SavePath, null);
        else
            File.Move(TempPath, SavePath);
    }

    public static SaveData Load()
    {
        if (!HasSave) return new SaveData();
        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogWarning("Сейв не прочитано: порожній об'єкт. Створено новий прогрес.");
                return new SaveData();
            }
            return Migrate(data);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Сейв не прочитано: {ex.Message}");
            return new SaveData();
        }
    }

    public static void Delete()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        if (File.Exists(TempPath)) File.Delete(TempPath);
    }

    private static SaveData Migrate(SaveData data)
    {
        if (data.version < 2)
        {
            data.levelIndex = 0;
            data.version = 2;
        }
        return data;
    }
}
