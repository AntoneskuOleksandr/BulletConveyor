using System.Collections.Generic;
using UnityEngine;

public class EveryNthDamageBoostModifier : IWeaponModifier
{
    private int _n;
    private float _multiplier;
    private int _counter;

    public void Initialize(Weapon weapon)
    {
        Debug.Log("[EveryNthDamageBoostModifier] Initialize");
        _n = Mathf.Max(1, (int)weapon.Data.GetParameter(WeaponParameterType.PowerfulShotEvery));
        _multiplier = Mathf.Max(1f, weapon.Data.GetParameter(WeaponParameterType.PowerShotMultiplier));
        _counter = 0;
    }

    public void OnBeforeShoot(Weapon weapon)
    {
        Debug.Log($"[EveryNthDamageBoostModifier] OnBeforeShoot _counter: {_counter}, _n: {_n}");
        _counter++;
        if (_counter > _n)
            _counter = 1;
    }

    public void OnBeforeBulletInit(Weapon weapon, ref BulletInitData data)
    {
        Debug.Log($"[EveryNthDamageBoostModifier] OnBeforeBulletInit _counter: {_counter}, _n: {_n}");
        if (_counter == _n)
        {
            data.Damage *= _multiplier;
            Debug.Log($"[{weapon.name}] PowerShot x{_multiplier}");
        }
    }

    public void OnBulletInit(Weapon weapon, Bullet bullet)
    {
        Debug.Log("[EveryNthDamageBoostModifier] OnBulletInit");
    }
    public void OnAfterShoot(Weapon weapon, List<Bullet> bullets)
    {
        Debug.Log("[EveryNthDamageBoostModifier] OnAfterShoot");
    }
    public void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit)
    {
        Debug.Log("[EveryNthDamageBoostModifier] OnBulletHit");
    }
}
