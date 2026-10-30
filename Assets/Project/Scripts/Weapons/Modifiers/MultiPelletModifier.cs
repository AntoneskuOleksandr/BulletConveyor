using UnityEngine;

public class MultiPelletModifier : IWeaponModifier, IStatProvider
{
    private int _pelletCount;
    private float _damageMultiplier;

    public void Initialize(Weapon weapon)
    {
        _pelletCount = Mathf.Max(1,
            weapon.Data.GetParameter<int>(WeaponParameterType.PelletsPerShot));

        _damageMultiplier =
            weapon.Data.GetParameter<float>(WeaponParameterType.PelletDamageMultiplier);
    }

    public void OnBeforeProjectileInit(Weapon weapon, ref ProjectileInitData data, Transform firePoint)
    {
        data.Damage *= _damageMultiplier;
    }

    public float ModifyRequiredProjectilesCount(float baseValue)
    {
        return baseValue + (_pelletCount - 1);
    }
}
