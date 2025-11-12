using System.Collections.Generic;
using UnityEngine;

public class MultiPelletModifier : IWeaponModifier
{
    private int _pelletCount;
    private float _damageMultiplier;

    public void Initialize(Weapon weapon)
    {
        _pelletCount = Mathf.Max(1, Mathf.RoundToInt(weapon.Data.GetParameter<int>(WeaponParameterType.PelletsPerShot)));
        _damageMultiplier = weapon.Data.GetParameter<float>(WeaponParameterType.PelletDamageMultiplier);
    }

    public void OnBeforeProjectileInit(Weapon weapon, ref ProjectileInitData data, Transform firePoint)
    {
        if (_pelletCount <= 1)
            return;
        data.Damage *= _damageMultiplier;
    }

    public IEnumerable<ProjectileInitData> Expand(Weapon weapon, ProjectileInitData baseData, Transform firePoint)
    {
        if (_pelletCount <= 1)
            yield break;

        for (int i = 0; i < _pelletCount - 1; i++)
        {
            ProjectileInitData clone = baseData;
            yield return clone;
        }
    }
}
