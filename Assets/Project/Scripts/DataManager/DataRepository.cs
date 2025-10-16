using System.IO;
using UnityEngine;

public class DataRepository
{
    private readonly string _path = Path.Combine(Application.persistentDataPath, "playerdata.json");

    public PlayerData Load()
    {
        if (!File.Exists(_path))
            return null;

        string json = File.ReadAllText(_path);
        return JsonUtility.FromJson<PlayerData>(json);
    }

    public void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_path, json);
    }
}
