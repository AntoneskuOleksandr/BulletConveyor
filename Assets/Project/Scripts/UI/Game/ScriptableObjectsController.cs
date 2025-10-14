using UnityEngine;

public class ScriptableObjectsController : MonoBehaviour
{
    [SerializeField] private ScriptableObject[] levels;
    [SerializeField] private LevelDisplay levelDisplay;
    private int currentIndex;

    private void Start()
    {
        currentIndex = PlayerPrefs.GetInt("CurrentLevel", 0) - 1;
        ChangeScriptableObject(0);
    }

    public void ChangeScriptableObject(int change)
    {
        currentIndex += change;
        if (currentIndex < 0) currentIndex = levels.Length - 1;
        else if (currentIndex > levels.Length - 1) currentIndex = 0;

        if (levelDisplay != null)
            levelDisplay.DisplayLevel((Level)levels[currentIndex]);
    }
}
