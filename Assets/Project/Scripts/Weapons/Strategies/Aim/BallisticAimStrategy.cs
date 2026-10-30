using UnityEngine;

public class BallisticAimStrategy : IAimStrategy
{
    private readonly bool _useHighArc = false;

    public AimResult ComputeAim(Weapon weapon)
    {
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        Transform target = weapon.Target.HitTransform;

        if (!firePoint || !target)
            return AimResult.Empty;

        float g = Mathf.Abs(Physics.gravity.y);
        float v = weapon.LaunchSpeed;

        Vector3 t = target.position;
        Vector3 d = t - firePoint.position;
        float x = new Vector3(d.x, 0f, d.z).magnitude;
        float y = d.y;

        float D = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);
        if (D < 0f)
            return AimResult.Empty;

        float sqrt = Mathf.Sqrt(D);
        float angle1 = Mathf.Atan((v * v + sqrt) / (g * x));
        float angle2 = Mathf.Atan((v * v - sqrt) / (g * x));

        float theta = _useHighArc ? Mathf.Max(angle1, angle2) : Mathf.Min(angle1, angle2);

        float targetThetaDeg = -theta * Mathf.Rad2Deg;

        return new AimResult
        {
            HasAim = true,
            ProvidesYaw = false,
            ProvidesPitch = true,
            PitchDeg = targetThetaDeg,
            AllowedErrorDeg = weapon.AimThreshold
        };
    }
}
