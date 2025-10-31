using UnityEngine;
using System.Collections.Generic;

public class SpreadShotModifier : IWeaponModifier
{
    private float _spreadAngle;

    public void Initialize(Weapon weapon)
    {
        _spreadAngle = weapon.Data.GetParameter(WeaponParameterType.SpreadAngle);
    }

    public void OnBeforeShoot(Weapon weapon) { }

    public void OnBeforeBulletInit(Weapon weapon, ref BulletInitData data)
    {
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        if (firePoint == null || weapon.Target == null) return;

        Vector3 baseDir = (weapon.Target.HitTransform.position - firePoint.position).normalized;

        Quaternion spreadRot = Quaternion.Euler(
            Random.Range(-_spreadAngle, _spreadAngle),
            Random.Range(-_spreadAngle, _spreadAngle),
            0f
        );

        Vector3 finalDir = (spreadRot * baseDir).normalized;

        data.OverrideForward = finalDir;
        data.HasOverrideForward = true;
    }

    public void OnBulletInit(Weapon weapon, Bullet bullet) { }
    public void OnAfterShoot(Weapon weapon, List<Bullet> bullets) { }
    public void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit) { }
}
