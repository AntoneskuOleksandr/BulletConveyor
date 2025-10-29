using UnityEngine;

public class BasicAimStrategy : IAimStrategy
{
    public AimResult ComputeAim(Weapon weapon)
    {
        var baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        var target = weapon.Target?.HitTransform;

        if (!baseRot || !target)
            return AimResult.Empty;

        Vector3 dir = target.position - baseRot.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return AimResult.Empty;

        float yawDeg = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        return new AimResult
        {
            HasAim = true,
            ProvidesYaw = true,
            ProvidesPitch = false,
            YawDeg = yawDeg,
            AllowedErrorDeg = weapon.AimThreshold
        };
    }
}
