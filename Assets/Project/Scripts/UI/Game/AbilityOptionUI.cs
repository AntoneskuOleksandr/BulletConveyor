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
    [SerializeField] private Button probabilityButton;
    [SerializeField] private GameObject probabilityDetailsPanel;
    [SerializeField] private TMP_Text detailsText;

    private AbilityData _data;
    private Action<AbilityData> _onSelected;
    private EffectChanceManager _effectChanceManager;

    public void Setup(AbilityData data, Action<AbilityData> onSelected, EffectChanceManager effectChanceManager)
    {
        _data = data;
        _onSelected = onSelected;
        _effectChanceManager = effectChanceManager;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (descriptionText != null) descriptionText.text = data.description;

        float nowTotal = _effectChanceManager != null
            ? _effectChanceManager.GetTotalChanceForEffect(_data.onHitEffectType)
            : 0f;

        float afterPick = Mathf.Clamp01(nowTotal + _data.probability);
        float delta = afterPick - nowTotal;

        if (probabilityText != null)
            probabilityText.text =
                $"{nowTotal * 100f:0.#}% → {afterPick * 100f:0.#}% (+{delta * 100f:0.#}%)";

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
        var current = _effectChanceManager.GetChancesForEffect(_data.onHitEffectType);

        var sb = new StringBuilder();
        sb.AppendLine($"Effect: {_data.onHitEffectType}");
        sb.AppendLine("Sources:");

        if (current.Count == 0)
        {
            sb.AppendLine(" - ");
        }
        else
        {
            foreach (var (source, value) in current)
            {
                sb.AppendLine($" - {source}: {value * 100f:0.#}%");
            }
        }

        return sb.ToString();
    }
}
