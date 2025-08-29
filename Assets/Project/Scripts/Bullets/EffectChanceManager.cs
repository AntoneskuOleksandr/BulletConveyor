using System.Collections.Generic;
using UnityEngine;

public class EffectChanceManager
{
    private List<ProjectileEffectChance> _chances = new List<ProjectileEffectChance>();

    public void RegisterEffectChance(ProjectileEffectChance chance) => _chances.Add(chance);
    public void Clear() => _chances.Clear();

    public List<OnHitEffectType> RollEffects()
    {
        List<OnHitEffectType> results = new List<OnHitEffectType>();
        foreach (var c in _chances)
        {
            if (Random.value < c.TotalChance) results.Add(c.EffectType);
        }
        return results;
    }

    [System.Serializable]
    public class ProjectileEffectChance
    {
        public OnHitEffectType EffectType;
        public float BaseChance;
        public float WeaponBonus;
        public float BuffBonus;
        public float EventBonus;

        public float TotalChance => Mathf.Clamp01(BaseChance + WeaponBonus + BuffBonus + EventBonus);
    }
}
