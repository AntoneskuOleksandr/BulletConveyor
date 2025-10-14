using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplay : MonoBehaviour
{
    [Header("MiniLevels")]
    [SerializeField] private List<LevelSO> levels;
    [SerializeField] private Transform miniLevels;

    [Header("UI Elements")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button previousLevelButton;

    [Header("Background")]
    [SerializeField] private BackgroundGenerator backgroundGenerator;

    private LevelSO currentLevel;
    private GameObject currentMiniLevel;

    private void OnEnable()
    {
        nextLevelButton.onClick.AddListener(() =>
        {
            int currentIndex = levels.IndexOf(currentLevel);
            int nextIndex = (currentIndex + 1) % levels.Count;
            ShowLevel(nextIndex);
        });
        previousLevelButton.onClick.AddListener(() =>
        {
            int currentIndex = levels.IndexOf(currentLevel);
            int previousIndex = (currentIndex - 1 + levels.Count) % levels.Count;
            ShowLevel(previousIndex);
        });

        if (levels.Count > 0)
            ShowLevel(0);

        miniLevels.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        nextLevelButton.onClick.RemoveAllListeners();
        previousLevelButton.onClick.RemoveAllListeners();

        if (miniLevels != null)
            miniLevels.gameObject.SetActive(false);
    }

    private void ShowLevel(int index)
    {
        if (currentLevel != null)
        {
            Destroy(currentMiniLevel);
        }

        currentMiniLevel = Instantiate(levels[index].MiniLevelGO, miniLevels);

        backgroundGenerator.SetColor(levels[index].GradientColorTop, levels[index].GradientColorBottom);

        currentLevel = levels[index];
    }
}
