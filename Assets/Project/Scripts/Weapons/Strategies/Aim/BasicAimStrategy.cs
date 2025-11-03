using UnityEngine;

public class BasicAimStrategy : IAimStrategy
{
    public AimResult ComputeAim(Weapon weapon)
    {
        Transform baseRot = weapon.Data.UseYaw
            ? weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation)
            : null;
        Transform barrelRot = weapon.Data.UsePitch
            ? weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation)
            : null;
        Transform target = weapon.Target?.HitTransform;

        if (target == null)
            return AimResult.Empty;
        if (weapon.Data.UseYaw && baseRot == null)
            return AimResult.Empty;
        if (weapon.Data.UsePitch && barrelRot == null)
            return AimResult.Empty;

        float yawDeg = 0f;
        if (weapon.Data.UseYaw)
        {
            Vector3 dir = target.position - baseRot.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                return AimResult.Empty;
            yawDeg = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        }

        float pitchDeg = 0f;
        if (weapon.Data.UsePitch)
        {
            Vector3 d = target.position - barrelRot.position;
            float xz = new Vector3(d.x, 0f, d.z).magnitude;
            float y = d.y;
            float pitchRad = Mathf.Atan2(y, xz);
            pitchDeg = -pitchRad * Mathf.Rad2Deg;
        }

        return new AimResult
        {
            HasAim = true,
            ProvidesYaw = weapon.Data.UseYaw,
            ProvidesPitch = weapon.Data.UsePitch,
            YawDeg = yawDeg,
            PitchDeg = pitchDeg,
            AllowedErrorDeg = weapon.AimThreshold
        };
    }
}
