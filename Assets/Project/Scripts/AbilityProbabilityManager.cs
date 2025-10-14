using System.Collections.Generic;
using UnityEngine;

public class AbilityProbabilityManager : MonoBehaviour
{
    public static AbilityProbabilityManager Instance;

    [SerializeField] private List<AbilityManager.Ability> abilities;
    [SerializeField] private List<float> probabilities;

    private Dictionary<AbilityManager.Ability, float> abilityProbabilities;

    private void Awake()
    {
        Instance = this;

        if (abilities.Count != probabilities.Count)
        {
            Debug.LogError("The number of abilities and probabilities do not match");
            return;
        }

        abilityProbabilities = new Dictionary<AbilityManager.Ability, float>();

        for (int i = 0; i < abilities.Count; i++)
        {
            abilityProbabilities.Add(abilities[i], probabilities[i]);
        }
    }

    public float GetProbability(AbilityManager.Ability ability)
    {
        if (abilityProbabilities.ContainsKey(ability))
        {
            return abilityProbabilities[ability];
        }
        else
        {
            Debug.LogError("The ability is not found in the dictionary of probabilities");
            return 0f;
        }
    }
}
