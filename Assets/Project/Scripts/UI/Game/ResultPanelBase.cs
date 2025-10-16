using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public abstract class ResultPanelBase : MonoBehaviour
{
    [SerializeField] protected TMP_Text gearsAmountText;
    [SerializeField] protected Button getButton;
    [SerializeField] protected string mainMenuSceneName = "MainMenu";

    [Inject] protected EconomyService _economyService;
    [Inject] protected PlayerRuntimeManager _playerRuntimeManager;

    protected virtual void OnEnable()
    {
        if (getButton != null)
            getButton.onClick.AddListener(OnGetButtonClicked);
    }

    public virtual void ShowPanel()
    {
        gameObject.SetActive(true);
        gearsAmountText.text = _playerRuntimeManager.GetCurrency(CurrencyType.Gears).ToString();
    }

    protected virtual void OnGetButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
        _economyService.AddCurrency(CurrencyType.Gears, _playerRuntimeManager.GetCurrency(CurrencyType.Gears));
        OnAfterReward();
    }

    protected abstract void OnAfterReward();
}
