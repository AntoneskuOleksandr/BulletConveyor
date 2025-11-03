public class IdleAimStrategy : IAimStrategy
{
    public AimResult ComputeAim(Weapon weapon)
    {
        var baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        if (!baseRot) return AimResult.Empty;

        return new AimResult
        {
            HasAim = true,
            ProvidesYaw = true,
            ProvidesPitch = true,
            YawDeg = 0,
            PitchDeg = weapon.Data.DefaultPitchAngle,
            AllowedErrorDeg = 1f
        };
    }
}
