using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Scriptable Objects/Level")]
public class LevelSO : ScriptableObject
{
    [Header("Menu Preview")]
    public GameObject MiniLevelGO;
    public int LevelIndex;
    public string LevelName;
    public Color GradientColorTop;
    public Color GradientColorBottom;

    [Header("Gameplay Config")]
    public LevelConfig levelConfig;
}
