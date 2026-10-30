using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PauseMenuPanel : MonoBehaviour
{
    [SerializeField] private GameObject abilitiesGrid;
    [SerializeField] private GameObject abilityIconPrefab;
    [SerializeField] private GameObject tutorialInstructions;
    private bool restoreTutorialInstructions;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Inject] private AbilityPanel _abilityPanel;
    [Inject] private GameSpeed _gameSpeed;

    private void Start()
    {
        resumeButton.onClick.AddListener(HidePanel);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ShowPanel()
    {
        if (gameObject.activeSelf) return;
        if (tutorialInstructions != null)
        {
            restoreTutorialInstructions = tutorialInstructions.activeSelf;
            tutorialInstructions.SetActive(false);
        }
        gameObject.SetActive(true);

        _gameSpeed.Pause(this);

        ShowSelectedAbilities();
    }

    private void HidePanel()
    {
        gameObject.SetActive(false);
        if (tutorialInstructions != null)
            tutorialInstructions.SetActive(restoreTutorialInstructions);
        _gameSpeed.Resume(this);
        ClearAbilityGrid();
    }

    private void ClearAbilityGrid()
    {
        if (abilitiesGrid != null)
        {
            foreach (Transform child in abilitiesGrid.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ShowSelectedAbilities()
    {
        if (abilitiesGrid != null)
        {
            abilitiesGrid.SetActive(true);
        }

        foreach (var ability in _abilityPanel.SelectedAbilities)
        {
            var icon = Instantiate(abilityIconPrefab, abilitiesGrid.transform);
            var iconImage = icon.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = ability.icon;
            }
        }
    }
}
