using System.Collections.Generic;
using UnityEngine;

public class EveryNthDamageBoostModifier : IWeaponModifier
{
    private int _n;
    private float _multiplier;
    private int _counter;

    public void Initialize(Weapon weapon)
    {
        _n = Mathf.Max(1, (int)weapon.Data.GetParameter(WeaponParameterType.PowerfulShotEvery));
        _multiplier = Mathf.Max(1f, weapon.Data.GetParameter(WeaponParameterType.PowerShotMultiplier));
        _counter = 0;
    }

    public void OnBeforeShoot(Weapon weapon) { }

    public void OnBeforeBulletInit(Weapon weapon, ref BulletInitData data)
    {
        _counter++;
        if (_counter > _n)
            _counter = 1;

        if (_counter == _n)
            data.Damage *= _multiplier;
    }


    public void OnBulletInit(Weapon weapon, Bullet bullet) { }
    public void OnAfterShoot(Weapon weapon, List<Bullet> bullets) { }
    public void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit) { }

    public IEnumerable<BulletInitData> Expand(Weapon weapon, BulletInitData baseData)
    {
        throw new System.NotImplementedException();
    }
}
