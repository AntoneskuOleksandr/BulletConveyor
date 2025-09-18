using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static EffectChanceManager;

[System.Serializable]
public class AbilityData
{
    public OnHitEffectType onHitEffectType;
    public Sprite icon;
    public string description;
    public float probability;
}

public class AbilityPanel : MonoBehaviour
{
    [SerializeField] private GameObject abilitiesRoot;
    [SerializeField] private GameObject abilityPrefab;

    [SerializeField] private List<AbilityData> abilities;
    [SerializeField] private int abilitiesCount = 3;

    [Inject] private EffectChanceManager _effectChanceManager;

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);
        Debug.Log("[AbilityPanel] ShowPanel called.");
        GenerateAbilities();
        Time.timeScale = 0f;
    }

    private void GenerateAbilities()
    {
        if (abilityPrefab == null || abilitiesRoot == null)
        {
            Debug.LogError("[AbilityPanel] Prefab or Panel is not assigned!");
            return;
        }

        if (abilities == null || abilities.Count == 0)
        {
            Debug.LogWarning("[AbilityPanel] No abilities assigned in inspector!");
            return;
        }

        for (int i = 0; i < abilitiesCount; i++)
        {
            GameObject ability = Instantiate(abilityPrefab, abilitiesRoot.transform, false);
            ability.name = $"Ability_{i}";

            Button button = ability.GetComponentInChildren<Button>(true);
            if (button == null)
            {
                Debug.LogWarning($"[{ability.name}] Button not found in prefab.");
                continue;
            }

            Image img = button.image ?? ability.GetComponentInChildren<Image>(true);
            if (img == null)
            {
                Debug.LogWarning($"[{ability.name}] Image component not found.");
                continue;
            }

            TMP_Text text = ability.GetComponentInChildren<TMP_Text>(true);
            if (text == null)
            {
                Debug.LogWarning($"[{ability.name}] Text component not found.");
                continue;
            }

            int randomIndex = Random.Range(0, abilities.Count);
            AbilityData data = abilities[randomIndex];

            img.sprite = data.icon;
            text.text = data.description;

            button.onClick.AddListener(() => OnAbilitySelected(data));
        }
    }
    private void OnAbilitySelected(AbilityData data)
    {
        Debug.Log($"[AbilityPanel] Ability selected: {data.onHitEffectType}");
        _effectChanceManager.RegisterEffectChance(
            data.onHitEffectType,
            data.probability,
            ChanceSource.Base
        );
        HidePanel();
    }


    public void HidePanel()
    {
        foreach (Transform child in abilitiesRoot.transform)
        {
            Destroy(child.gameObject);
        }
        this.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
