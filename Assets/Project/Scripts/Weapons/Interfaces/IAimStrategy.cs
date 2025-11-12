public interface IAimStrategy
{
    /// <summary>
    /// Пытается навестись на цель. 
    /// Возвращает true, если пушка в пределах допустимого угла и прицелилась.
    /// </summary>
    AimResult ComputeAim(Weapon weapon);
}

public struct AimResult
{
    public bool HasAim;
    public float YawDeg;      // Глобальный yaw (по оси Y мира)
    public float PitchDeg;    // Локальный pitch (по оси X дула)
    public bool ProvidesYaw;  // Стратегия управляет yaw
    public bool ProvidesPitch;// Стратегия управляет pitch
    public float AllowedErrorDeg;

    public static AimResult Empty => new AimResult
    {
        HasAim = false,
        ProvidesYaw = false,
        ProvidesPitch = false,
        AllowedErrorDeg = 0f
    };
}


