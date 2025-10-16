using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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

    [Inject] private PlayerProgressService _playerProgressService;
    [Inject] private MainMenuUIManager _mainMenuUIManager;

    private LevelSO currentLevel;
    private GameObject currentMiniLevel;

    public event Action OnLevelChanged;

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

        ShowLevel(_playerProgressService.GetSelectedLevel());

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

        _mainMenuUIManager.SetLockPanelState(levels[index].LevelIndex > _playerProgressService.GetCurrentLevel());

        OnLevelChanged?.Invoke();
    }

    public int GetCurrentLevel()
    {
        return currentLevel.LevelIndex;
    }
}
