public interface IAimStrategy
{

    AimResult ComputeAim(Weapon weapon);
}

public struct AimResult
{
    public bool HasAim;
    public float YawDeg;
    public float PitchDeg;
    public bool ProvidesYaw;
    public bool ProvidesPitch;
    public float AllowedErrorDeg;

    public static AimResult Empty => new AimResult
    {
        HasAim = false,
        ProvidesYaw = false,
        ProvidesPitch = false,
        AllowedErrorDeg = 0f
    };
}

