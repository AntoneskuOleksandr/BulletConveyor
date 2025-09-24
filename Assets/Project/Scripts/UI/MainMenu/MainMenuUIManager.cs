using TMPro;
using UnityEngine;
using Zenject;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

    [Inject] private EconomyManager _economy;

    private void Awake()
    {
        _economy.OnMoneyChanged += UpdateMoneyDisplay;

        UpdateMoneyDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        moneyText.text = _economy.Money.ToString();
    }
}
