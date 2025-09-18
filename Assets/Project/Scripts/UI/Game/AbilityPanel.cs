using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AbilityData
{
    public string name;
    public Sprite icon;
    public string description;
}

public class AbilityPanel : MonoBehaviour
{
    [SerializeField] private GameObject abilitiesRoot;
    [SerializeField] private GameObject abilityPrefab;

    [SerializeField] private List<AbilityData> abilities;
    [SerializeField] private int abilitiesCount = 3;

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

            button.onClick.AddListener(() =>
            {
                Debug.Log($"[{ability.name}] Ability selected.");
                Time.timeScale = 1f;
                this.gameObject.SetActive(false);
                foreach (Transform child in abilitiesRoot.transform)
                {
                    Destroy(child.gameObject);
                }
            });

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
        }
    }
}
