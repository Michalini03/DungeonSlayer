using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "run.json");

    public static void SaveRun(RunSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static RunSaveData LoadRun()
    {
        if (!File.Exists(SavePath))
        {
            return null;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<RunSaveData>(json);
    }

    public static bool HasRunSave()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteRun()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}