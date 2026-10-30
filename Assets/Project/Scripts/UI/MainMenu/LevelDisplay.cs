using System;
using TMPro;
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

    [SerializeField] private TMP_Text levelNameText;
    private LevelSO currentLevel;
    private GameObject currentMiniLevel;
    private int currentLevelIndex;

    public event Action OnLevelChanged;

    private bool subscribed;

    private void Start() { if (!subscribed) OnEnable(); }

    private void OnEnable()
    {
        if (_playerProgressService == null || _mainMenuUIManager == null || subscribed) return;
        subscribed = true;
        nextLevelButton.onClick.AddListener(ShowNextLevel);
        previousLevelButton.onClick.AddListener(ShowPreviousLevel);

        if (levels.Count == 0)
        {
            currentLevel = null;
            nextLevelButton.interactable = false;
            previousLevelButton.interactable = false;
            OnLevelChanged?.Invoke();
            return;
        }

        int selectedLevel = _playerProgressService.GetSelectedLevel();
        int selectedIndex = levels.FindIndex(level => level.LevelIndex == selectedLevel);
        ShowLevel(Mathf.Max(0, selectedIndex));

        miniLevels.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (!subscribed) return;
        subscribed = false;
        nextLevelButton.onClick.RemoveListener(ShowNextLevel);
        previousLevelButton.onClick.RemoveListener(ShowPreviousLevel);

        if (miniLevels != null)
            miniLevels.gameObject.SetActive(false);
    }

    private void ShowLevel(int index)
    {
        LevelSO level = levels[index];
        if (currentLevel != level || currentMiniLevel == null)
        {
            if (currentMiniLevel != null)
                Destroy(currentMiniLevel);
            currentMiniLevel = Instantiate(level.MiniLevelGO, miniLevels);
        }

        currentLevel = level;
        if (levelNameText != null) levelNameText.text = level.LevelName.ToUpperInvariant();
        currentLevelIndex = index;
        backgroundGenerator.SetColor(level.GradientColorTop, level.GradientColorBottom);

        _mainMenuUIManager.SetLockPanelState(levels[index].LevelIndex > _playerProgressService.GetCurrentLevel());

        OnLevelChanged?.Invoke();
    }

    private void ShowNextLevel()
    {
        if (levels.Count > 0)
            ShowLevel((currentLevelIndex + 1) % levels.Count);
    }

    private void ShowPreviousLevel()
    {
        if (levels.Count > 0)
            ShowLevel((currentLevelIndex - 1 + levels.Count) % levels.Count);
    }

    public bool TryGetCurrentLevel(out int levelIndex)
    {
        levelIndex = currentLevel != null ? currentLevel.LevelIndex : default;
        return currentLevel != null;
    }

    public int GetCurrentLevel()
    {
        return currentLevel != null ? currentLevel.LevelIndex : _playerProgressService.GetSelectedLevel();
    }
}
