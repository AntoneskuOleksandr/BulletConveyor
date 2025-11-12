using UnityEngine;

public class SpreadShotModifier : IWeaponModifier
{
    private float _spreadAngle;

    public void Initialize(Weapon weapon)
    {
        _spreadAngle = weapon.Data.GetParameter<float>(WeaponParameterType.SpreadAngle);
    }

    public void OnBeforeProjectileInit(Weapon weapon, ref ProjectileInitData data, Transform firePoint)
    {
        if (firePoint == null)
            return;

        Vector3 baseDir;
        if (weapon.Target != null && weapon.Target.HitTransform != null)
        {
            baseDir = (weapon.Target.HitTransform.position - firePoint.position).normalized;
        }
        else
        {
            baseDir = firePoint.forward;
        }

        float yaw = Random.Range(-_spreadAngle, _spreadAngle);
        float pitch = Random.Range(-_spreadAngle, _spreadAngle);
        Quaternion spreadRot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 finalDir = (spreadRot * baseDir).normalized;
        data.OverrideForward = finalDir;
        data.HasOverrideForward = true;
    }
}
