using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    public bool giveAllAbilities;

    public List<Ability> selectedAbilities = new List<Ability>();

    public enum Ability { None, FireBullet, FreezeBullet, DeadBullet, PoisonBullet, StunBullet, DamageIncreasedBullet, ExlosionBullet, FireInRadius, FreezeInRadius, PoisonInRadius, AddBulletsToGenerate }

    public UnityEvent OnBulletToGenerateAbilitySelected;

    private List<Ability> randomAbilities = new List<Ability>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(giveAllAbilities)
        {
        SelectAbility(Ability.FireBullet);
        SelectAbility(Ability.FreezeBullet);
        SelectAbility(Ability.DeadBullet);
        SelectAbility(Ability.PoisonBullet);
        SelectAbility(Ability.StunBullet);
        SelectAbility(Ability.DamageIncreasedBullet);
        SelectAbility(Ability.ExlosionBullet);
        SelectAbility(Ability.FireInRadius);
        SelectAbility(Ability.FreezeInRadius);
        SelectAbility(Ability.PoisonInRadius);
        SelectAbility(Ability.AddBulletsToGenerate);
        }
    }

    public List<Ability> GetRandomAbilities(int count, bool onlyBulletAbility = false)
    {
        randomAbilities.Clear();

        List<Ability> allAbilities = new List<Ability>() { Ability.FireBullet, Ability.FreezeBullet, Ability.DeadBullet, Ability.PoisonBullet, Ability.StunBullet, Ability.DamageIncreasedBullet, Ability.ExlosionBullet, Ability.FireInRadius, Ability.FreezeInRadius, Ability.PoisonInRadius, Ability.AddBulletsToGenerate };
        if (onlyBulletAbility)
            allAbilities = new List<Ability>() { Ability.FireBullet, Ability.FreezeBullet, Ability.DeadBullet, Ability.PoisonBullet, Ability.StunBullet, Ability.DamageIncreasedBullet, Ability.ExlosionBullet };

        allAbilities.RemoveAll(ability => selectedAbilities.Contains(ability));

        randomAbilities = new List<Ability>();

        for (int i = 0; i < count; i++)
        {
            if (allAbilities.Count == 0)
                break;

            int randomIndex = Random.Range(0, allAbilities.Count);
            randomAbilities.Add(allAbilities[randomIndex]);
            allAbilities.RemoveAt(randomIndex);
        }

        return randomAbilities;
    }

    public void SelectAbility(Ability ability)
    {
        if (ability == Ability.FreezeInRadius)
            Spawner.Instance.hasFreezeInRadiusEffect = true;
        else if (ability == Ability.FireInRadius)
            Spawner.Instance.hasFireInRadiusEffect = true;
        else if (ability == Ability.PoisonInRadius)
            Spawner.Instance.hasPoisonInRadiusEffect = true;
        else if (ability == Ability.AddBulletsToGenerate)
            OnBulletToGenerateAbilitySelected.Invoke();


        selectedAbilities.Add(ability);
        UIManager.Instance.HideAbilityScreen();
    }

    public void SelectAllAbilities()
    {
        foreach (Ability ability in randomAbilities)
            SelectAbility(ability);
    }
}



