using System.Collections.Generic;
using UnityEngine;
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

    public List<AbilityData> SelectedAbilities => _selectedAbilities;

    private readonly List<AbilityData> _selectedAbilities = new();


    public void ShowPanel()
    {
        gameObject.SetActive(true);
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

        // Копируем пул и ограничиваем по abilitiesCount
        List<AbilityData> pool = new List<AbilityData>(abilities);
        int count = Mathf.Min(abilitiesCount, pool.Count);

        for (int i = 0; i < count; i++)
        {
            GameObject abilityGO = Instantiate(abilityPrefab, abilitiesRoot.transform, false);
            abilityGO.name = $"Ability_{i}";

            int randomIndex = Random.Range(0, pool.Count);
            AbilityData data = pool[randomIndex];
            pool.RemoveAt(randomIndex);

            AbilityOptionUI optionUI = abilityGO.GetComponent<AbilityOptionUI>();
            if (optionUI == null)
            {
                Debug.LogError($"[{abilityGO.name}] AbilityOptionUI component not found!");
                continue;
            }

            optionUI.Setup(data, OnAbilitySelected, _effectChanceManager);
        }
    }

    private void OnAbilitySelected(AbilityData data)
    {
        _selectedAbilities.Add(data);

        Debug.Log($"[AbilityPanel] Ability selected: {data.onHitEffectType}");
        _effectChanceManager.RegisterGlobalChance(
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
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
