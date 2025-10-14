using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Scriptable Objects/Level")]
public class LevelSO : ScriptableObject
{
    public GameObject MiniLevelGO;
    public int LevelIndex;
    public string LevelName;
    public string SceneToLoad;
    public Color GradientColorTop;
    public Color GradientColorBottom;
}

