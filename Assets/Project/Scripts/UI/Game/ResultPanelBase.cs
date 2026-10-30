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
    private bool rewardClaimed;

    protected virtual void OnEnable()
    {
        if (getButton != null)
            getButton.onClick.AddListener(OnGetButtonClicked);
    }

    protected virtual void OnDisable()
    {
        if (getButton != null)
            getButton.onClick.RemoveListener(OnGetButtonClicked);
    }

    public virtual void ShowPanel()
    {
        gameObject.SetActive(true);
        gearsAmountText.text = _playerRuntimeManager.GetCurrency(CurrencyType.Gears).ToString();
    }

    protected virtual void OnGetButtonClicked()
    {
        if (rewardClaimed) return;
        rewardClaimed = true;
        _economyService.AddCurrency(CurrencyType.Gears, _playerRuntimeManager.GetCurrency(CurrencyType.Gears));
        OnAfterReward();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    protected abstract void OnAfterReward();
}
