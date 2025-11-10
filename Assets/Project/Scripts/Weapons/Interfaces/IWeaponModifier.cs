using System.Collections.Generic;
using UnityEngine;

public interface IWeaponModifier
{
    void Initialize(Weapon weapon);
    void OnBeforeShoot(Weapon weapon);
    void OnBeforeBulletInit(Weapon weapon, ref BulletInitData data, Transform firePoint);
    void OnBulletInit(Weapon weapon, Bullet bullet);
    void OnAfterShoot(Weapon weapon, List<Bullet> bullets);
    void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit);
    IEnumerable<BulletInitData> Expand(Weapon weapon, BulletInitData baseData, Transform firePoint);
}
