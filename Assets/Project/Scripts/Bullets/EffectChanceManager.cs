using System.Collections.Generic;
using UnityEngine;

public class EffectChanceManager
{
    private List<ProjectileEffectChance> _chances = new List<ProjectileEffectChance>();

    public void RegisterEffectChance(ProjectileEffectChance chance) => _chances.Add(chance);
    public void Clear() => _chances.Clear();

    public List<ProjectileEffectType> RollEffects()
    {
        List<ProjectileEffectType> results = new List<ProjectileEffectType>();
        foreach (var c in _chances)
        {
            if (Random.value < c.TotalChance) results.Add(c.EffectType);
        }
        return results;
    }

    [System.Serializable]
    public class ProjectileEffectChance
    {
        public ProjectileEffectType EffectType;
        public float BaseChance;
        public float WeaponBonus;
        public float BuffBonus;
        public float EventBonus;

        public float TotalChance => Mathf.Clamp01(BaseChance + WeaponBonus + BuffBonus + EventBonus);
    }

    public enum ProjectileEffectType
    {
        Fire,
        Ice,
        Explosion,
        Poison,
        // новые эффекты легко добавлять сюда
    }
}
