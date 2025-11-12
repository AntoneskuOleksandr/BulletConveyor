using UnityEngine;

public class BasicAimStrategy : IAimStrategy
{
    public AimResult ComputeAim(Weapon weapon)
    {
        var baseRot = weapon.Data.UseYaw
            ? weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation)
            : null;
        var barrels = weapon.Data.UsePitch
            ? weapon.AttachmentPoints.GetAll(WeaponAttachmentType.BarrelRotation)
            : null;
        var target = weapon.Target?.HitTransform;

        if (target == null)
            return AimResult.Empty;
        if (weapon.Data.UseYaw && baseRot == null)
            return AimResult.Empty;
        if (weapon.Data.UsePitch && (barrels == null || barrels.Count == 0))
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

        float totalPitch = 0f;
        int count = 0;

        if (weapon.Data.UsePitch)
        {
            foreach (var barrel in barrels)
            {
                Vector3 d = target.position - barrel.position;
                float xz = new Vector3(d.x, 0f, d.z).magnitude;
                float y = d.y;
                float pitchRad = Mathf.Atan2(y, xz);
                totalPitch += -pitchRad * Mathf.Rad2Deg;
                count++;
            }
        }

        float pitchDeg = (count > 0) ? totalPitch / count : 0f;

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
