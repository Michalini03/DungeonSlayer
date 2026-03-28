using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static void SaveRun(RunSaveData data)
    {
        string path = Path.Combine(Application.persistentDataPath, "run.json");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static RunSaveData LoadRun()
    {
        string path = Path.Combine(Application.persistentDataPath, "run.json");

        if (!File.Exists(path))
        {
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<RunSaveData>(json);
    }
}