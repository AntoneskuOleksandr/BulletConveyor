using System.Collections.Generic;
using UnityEngine;

public class SpreadShotModifier : IWeaponModifier
{
    private float _spreadAngle;

    public void Initialize(Weapon weapon)
    {
        _spreadAngle = weapon.Data.GetParameter(WeaponParameterType.SpreadAngle);
    }

    public void OnBeforeShoot(Weapon weapon) { }

    public void OnBeforeBulletInit(Weapon weapon, ref BulletInitData data, Transform firePoint)
    {
        if (firePoint == null)
            return;

        // –ассчитываем базовое направление Ч от firePoint к цели (если есть)
        Vector3 baseDir;
        if (weapon.Target != null && weapon.Target.HitTransform != null)
        {
            baseDir = (weapon.Target.HitTransform.position - firePoint.position).normalized;
        }
        else
        {
            baseDir = firePoint.forward;
        }

        // ƒобавл€ем случайный разброс
        float yaw = Random.Range(-_spreadAngle, _spreadAngle);
        float pitch = Random.Range(-_spreadAngle, _spreadAngle);
        Quaternion spreadRot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 finalDir = (spreadRot * baseDir).normalized;
        data.OverrideForward = finalDir;
        data.HasOverrideForward = true;
    }

    public void OnBulletInit(Weapon weapon, Bullet bullet) { }
    public void OnAfterShoot(Weapon weapon, List<Bullet> bullets) { }
    public void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit) { }

    public IEnumerable<BulletInitData> Expand(Weapon weapon, BulletInitData baseData, Transform firePoint)
    {
        yield break;
    }
}
