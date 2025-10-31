using System.Collections.Generic;
using UnityEngine;

public class EveryNthDamageBoostModifier : IWeaponModifier
{
    private int _n;
    private float _multiplier;
    private int _counter;

    public void Initialize(Weapon weapon) { }

    public void OnBeforeShoot(Weapon weapon) { }

    public void OnAfterShoot(Weapon weapon, List<Bullet> bullets) { }

    public void OnBulletInit(Weapon weapon, Bullet bullet)
    {
        _counter++;
        if (_counter >= _n)
        {
            _counter = 0;
            //bullet.SetDamage(bullet.Damage * _multiplier);
            Debug.Log($"[{weapon.name}] Boosted bullet x{_multiplier}");
        }
    }

    public void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit) { }
}
