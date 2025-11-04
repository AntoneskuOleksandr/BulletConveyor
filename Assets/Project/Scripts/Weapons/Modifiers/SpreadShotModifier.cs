using UnityEngine;
using System.Collections.Generic;

public class SpreadShotModifier : IWeaponModifier
{
    private float _spreadAngle;
    private int _firePointIndex;

    public void Initialize(Weapon weapon)
    {
        _spreadAngle = weapon.Data.GetParameter(WeaponParameterType.SpreadAngle);
        _firePointIndex = 0;
    }

    public void OnBeforeShoot(Weapon weapon)
    {
        _firePointIndex = 0;
    }

    public void OnBeforeBulletInit(Weapon weapon, ref BulletInitData data)
    {
        var firePoints = weapon.AttachmentPoints.GetAll(WeaponAttachmentType.FirePoint);
        if (firePoints == null || firePoints.Count == 0 || weapon.Target == null)
            return;

        Transform firePoint = firePoints[_firePointIndex % firePoints.Count];
        _firePointIndex++;

        Transform target = weapon.Target.HitTransform;
        if (target == null)
            return;

        Vector3 baseDir = (target.position - firePoint.position).normalized;

        float randomYaw = Random.Range(-_spreadAngle, _spreadAngle);
        float randomPitch = Random.Range(-_spreadAngle, _spreadAngle);
        Quaternion spreadRot = Quaternion.Euler(randomPitch, randomYaw, 0f);

        Vector3 finalDir = (spreadRot * baseDir).normalized;

        data.OverrideForward = finalDir;
        data.HasOverrideForward = true;
    }

    public void OnBulletInit(Weapon weapon, Bullet bullet) { }
    public void OnAfterShoot(Weapon weapon, List<Bullet> bullets) { }
    public void OnBulletHit(Weapon weapon, Bullet bullet, Collider hit) { }

    public IEnumerable<BulletInitData> Expand(Weapon weapon, BulletInitData baseData)
    {
        return null;
    }
}
