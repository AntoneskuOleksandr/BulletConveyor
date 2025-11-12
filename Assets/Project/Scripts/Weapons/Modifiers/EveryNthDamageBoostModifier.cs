using UnityEngine;

public class EveryNthDamageBoostModifier : IWeaponModifier
{
    private int _n;
    private float _multiplier;
    private int _counter;

    public void Initialize(Weapon weapon)
    {
        _n = Mathf.Max(1, weapon.Data.GetParameter<int>(WeaponParameterType.PowerfulShotEvery));
        _multiplier = Mathf.Max(1f, weapon.Data.GetParameter<float>(WeaponParameterType.PowerShotMultiplier));
        _counter = 0;
    }

    public void OnBeforeProjectileInit(Weapon weapon, ref ProjectileInitData data, Transform firePoint)
    {
        _counter++;
        if (_counter > _n)
            _counter = 1;

        if (_counter == _n)
        {
            data.Damage *= _multiplier;
        }
    }
}
