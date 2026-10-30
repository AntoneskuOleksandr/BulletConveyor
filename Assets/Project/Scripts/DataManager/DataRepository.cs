using System.IO;
using System;
using UnityEngine;

public class DataRepository
{
    private readonly string _path;
#if UNITY_EDITOR
    public static string ValidationPath;
#endif

    public DataRepository(string path = null)
    {
#if UNITY_EDITOR
        path ??= ValidationPath;
#endif
        _path = path ?? Path.Combine(Application.persistentDataPath, "playerdata.json");
    }

    public PlayerData Load()
    {
        if (!File.Exists(_path))
            return null;

        try
        {
            return JsonUtility.FromJson<PlayerData>(File.ReadAllText(_path));
        }
        catch (Exception exception) when (exception is ArgumentException || exception is IOException)
        {
            File.Move(_path, _path + ".corrupt-" + DateTime.UtcNow.Ticks);
            if (File.Exists(_path + ".bak"))
                return JsonUtility.FromJson<PlayerData>(File.ReadAllText(_path + ".bak"));
            return null;
        }
    }

    public void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        Directory.CreateDirectory(Path.GetDirectoryName(_path));
        string temporary = _path + ".tmp";
        File.WriteAllText(temporary, json);
        if (File.Exists(_path)) File.Replace(temporary, _path, _path + ".bak");
        else File.Move(temporary, _path);
    }
}
