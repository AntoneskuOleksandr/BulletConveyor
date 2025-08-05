using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Scriptable Objects/Level")]
public class Level : ScriptableObject
{
    public int levelIndex;
    public string levelName;
    public string sceneToLoad;
    public Color gradientColorTop;
    public Color gradientColorBottom;
}
