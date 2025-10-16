using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PauseMenuPanel : MonoBehaviour
{
    [SerializeField] private GameObject abilitiesGrid;
    [SerializeField] private GameObject abilityIconPrefab;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Inject] private AbilityPanel _abilityPanel;

    private void Start()
    {
        resumeButton.onClick.AddListener(HidePanel);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void OnRestartButtonClicked()
    {
        Time.timeScale = 1f; // Resume the game before restarting
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f; // Resume the game before going to main menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // Ensure "MainMenu" matches your main menu scene name
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);

        Time.timeScale = 0f; // Pause the game

        ShowSelectedAbilities();
    }

    private void HidePanel()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f; // Resume the game
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
