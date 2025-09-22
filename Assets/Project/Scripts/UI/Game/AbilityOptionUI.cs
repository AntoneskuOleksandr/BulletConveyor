using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityOptionUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text probabilityText;

    [Header("Details")]
    [SerializeField] private Button probabilityButton;           // кнопка для раскрытия деталей
    [SerializeField] private GameObject probabilityDetailsPanel; // панель с формулой
    [SerializeField] private TMP_Text detailsText;               // текст внутри панели

    private AbilityData _data;
    private Action<AbilityData> _onSelected;

    // для запроса данных из менеджера
    private EffectChanceManager _effectChanceManager;

    public void Setup(AbilityData data, Action<AbilityData> onSelected, EffectChanceManager effectChanceManager)
    {
        _data = data;
        _onSelected = onSelected;
        _effectChanceManager = effectChanceManager;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (descriptionText != null) descriptionText.text = data.description;
        if (probabilityText != null) probabilityText.text = $"{data.probability * 100f:0.#}%";

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => _onSelected?.Invoke(_data));

        if (probabilityButton != null)
        {
            probabilityButton.onClick.RemoveAllListeners();
            probabilityButton.onClick.AddListener(ToggleProbabilityDetails);
        }

        if (probabilityDetailsPanel != null)
            probabilityDetailsPanel.SetActive(false);
    }

    private void ToggleProbabilityDetails()
    {
        if (probabilityDetailsPanel == null) return;

        bool active = !probabilityDetailsPanel.activeSelf;
        probabilityDetailsPanel.SetActive(active);

        if (active && detailsText != null)
        {
            detailsText.text = BuildDetails();
        }
    }

    private string BuildDetails()
    {
        if (_effectChanceManager == null)
            return $"Base chance: {_data.probability * 100f:0.#}%";

        var chances = _effectChanceManager.GetChancesForEffect(_data.onHitEffectType);

        float total = 0f;
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"Effect: {_data.onHitEffectType}");
        sb.AppendLine("Sources:");

        foreach (var (source, value) in chances)
        {
            sb.AppendLine($" - {source}: {value * 100f:0.#}%");
            total += value;
        }

        sb.AppendLine("--------------------");
        sb.AppendLine($"Total: {Mathf.Clamp01(total) * 100f:0.#}%");

        return sb.ToString();
    }

}
