using TMPro;
using UnityEngine;
using Zenject;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

    [Inject] private EconomyService _economy;

    private void Start()
    {
        _economy.OnCurrencyChanged += UpdateMoneyDisplay;

        UpdateMoneyDisplay();
    }

    private void OnDestroy()
    {
        _economy.OnCurrencyChanged -= UpdateMoneyDisplay;
    }

    private void UpdateMoneyDisplay()
    {
        moneyText.text = _economy.Currency.ToString();
    }
}
