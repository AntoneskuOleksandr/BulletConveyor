using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class LosePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text gearsAmountText;
    [SerializeField] private Button getButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Inject] private EconomyService _economyService;
    [Inject] private PlayerRuntimeManager _playerRuntimeManager;

    private void OnEnable()
    {
        if (getButton != null)
        {
            getButton.onClick.AddListener(OnGetButtonClicked);
        }
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        gearsAmountText.text = _playerRuntimeManager.GetCurrency(CurrencyType.Gears).ToString();
    }

    private void OnGetButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
        _economyService.AddCurrency(CurrencyType.Gears, _playerRuntimeManager.GetCurrency(CurrencyType.Gears));
    }
}
