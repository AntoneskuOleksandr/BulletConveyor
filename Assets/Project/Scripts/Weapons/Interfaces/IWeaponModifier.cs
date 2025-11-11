using System.Collections.Generic;
using UnityEngine;

public interface IWeaponModifier
{
    void Initialize(Weapon weapon);
    void OnBeforeShoot(Weapon weapon);
    void OnBeforeProjectileInit(Weapon weapon, ref ProjectileInitData data, Transform firePoint);
    void OnProjectileInit(Weapon weapon, ProjectileBase projectile);
    void OnAfterShoot(Weapon weapon, List<ProjectileBase> projectiles);
    void OnProjectileHit(Weapon weapon, ProjectileBase projectile, Collider hit);
    IEnumerable<ProjectileInitData> Expand(Weapon weapon, ProjectileInitData baseData, Transform firePoint);
}
