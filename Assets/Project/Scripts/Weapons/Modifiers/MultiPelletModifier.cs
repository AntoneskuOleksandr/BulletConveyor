using System.Collections.Generic;
using UnityEngine;

public class MultiPelletModifier : IWeaponModifier
{
    private int _pelletCount;
    private float _damageMultiplier;

    public void Initialize(Weapon weapon)
    {
        _pelletCount = Mathf.Max(1, Mathf.RoundToInt(weapon.Data.GetParameter(WeaponParameterType.PelletsPerShot)));
        _damageMultiplier = weapon.Data.GetParameter(WeaponParameterType.PelletDamageMultiplier);
    }

    public IEnumerable<BulletInitData> Expand(Weapon weapon, BulletInitData baseData, Transform firePoint)
    {
        if (_pelletCount <= 1)
            yield break;

        for (int i = 0; i < _pelletCount - 1; i++)
        {
            BulletInitData clone = baseData;
            clone.Damage *= _damageMultiplier;
            yield return clone;
        }
    }

    public void OnBeforeShoot(Weapon weapon) { }

    public void OnBeforeBulletInit(Weapon weapon, ref BulletInitData data, Transform firePoint) { }

    public void OnBulletInit(Weapon weapon, Bullet bullet) { }

    public void OnAfterShoot(Weapon weapon, List<Bullet> bullets) { }

    public void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit) { }
}
