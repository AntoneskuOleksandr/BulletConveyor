using System.Collections.Generic;
using UnityEngine;

public interface IWeaponModifier
{
    void Initialize(Weapon weapon);
    void OnBeforeShoot(Weapon weapon);
    void OnAfterShoot(Weapon weapon, List<Bullet> bullets);
    void OnBulletInit(Weapon weapon, Bullet bullet);
    void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit);
}