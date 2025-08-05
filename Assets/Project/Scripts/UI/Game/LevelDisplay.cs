using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text mapName;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject lockImage;
    [SerializeField] private GameObject[] levelPrefabs;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private GradientCreator gradientCreator;

    private Level currentLevel;
    private GameObject[] levelsToShow;

    private void Awake()
    {
        levelsToShow = new GameObject[levelPrefabs.Length];

        for (int i = 0; i < levelPrefabs.Length; i++)
        {
            GameObject level;

            level = Instantiate(levelPrefabs[i], spawnPosition.position, Quaternion.identity, spawnPosition);
            level.SetActive(false);

            levelsToShow[i] = level;
        }
    }

    private void OnDisable()
    {
        if (spawnPosition != null && spawnPosition.gameObject.activeSelf)
            spawnPosition.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (spawnPosition != null)
            spawnPosition.gameObject.SetActive(true);
        else
            Debug.LogError("SpawnPosition is null");
    }

    public void DisplayLevel(Level level)
    {
        if (level == null)
        {
            Debug.LogError("Level is null");
            return;
        }

        currentLevel = level;
        mapName.text = level.levelName;

        for (int i = 0; i < levelsToShow.Length; i++)
        {
            if (levelsToShow[i] != null)
                levelsToShow[i].SetActive(i == level.levelIndex);
            else
                Debug.LogError("LevelToShow[" + i + "] is null");
        }

        bool levelUnlocked = PlayerPrefs.GetInt("CurrentLevel", 0) >= level.levelIndex + 1 && MainMenuManager.Instance.IsHasAnyGun();

        lockImage.SetActive(!levelUnlocked);

        gradientCreator.SetColor(level.gradientColorBottom, level.gradientColorTop);

        playButton.interactable = levelUnlocked;
        playButton.onClick.RemoveAllListeners();

        if (level.sceneToLoad != null)
            playButton.onClick.AddListener(() => SceneManager.LoadScene(level.sceneToLoad));
        else
            Debug.LogError("SceneToLoad is null");
    }

    public void UpdateCurrentLevelState()
    {
        DisplayLevel(currentLevel);
    }
}
