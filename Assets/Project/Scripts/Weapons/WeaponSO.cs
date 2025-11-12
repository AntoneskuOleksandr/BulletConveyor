using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeLevel
{
    public int Cost;
    public int DamageBonus;
    public float FireRateBonus;
    public int HealthBonus;
}

public enum WeaponParameterType
{
    AoERadius = 0,
    SpreadAngle = 1,
    PowerfulShotEvery = 2,
    ShootSimultaneously = 3, // 1 - simultaneously, 0 - sequentially
    PowerShotMultiplier = 4,
    PelletsPerShot = 5,
    PelletDamageMultiplier = 6,
    LifeTime = 7,
    BeamWidth = 8,
    Length = 9,
    PierceCount = 10,
    Color = 11,
    TickInterval = 12,
    FireRateIncreasePerShot = 13,
    MaxFireRateBonus = 14,
    FireRateRampResetTime = 15
}

public enum WeaponParameterValueType
{
    Float,
    Int,
    Bool,
    Color,
    Vector3
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("General Info")]
    public int WeaponId;
    public GameObject WeaponGO;
    public int RequiredLevel;
    public WeaponType WeaponType;

    [Header("Base Stats")]
    public int BaseDamage;
    public float BaseFireRate;
    public int BaseHealth;

    [Header("Attack Settings")]
    public float MaxAttackRadius;
    public float MinAttackRadius;
    public float FindTargetRadiusCoefficient;

    [Header("Aiming Settings")]
    public float AimThreshold;

    // Yaw
    public bool UseYaw;
    public float YawTurnSpeed;
    // Pitch
    public bool UsePitch;
    public float PitchTurnSpeed;
    public float MinPitchAngle;
    public float MaxPitchAngle;
    public float DefaultPitchAngle;

    [Header("Projectile Settings")]
    [Min(1)] public int MinProjectilesToShoot = 1;
    public LaunchSpeedMode LaunchSpeedMode = LaunchSpeedMode.Fixed;
    public float LaunchSpeed = 10f;

    [Header("Strategies")]
    public List<AimStrategyType> AimStrategies;
    public ShootStrategyType ShootStrategy;
    public TargetStrategyType TargetStrategy;
    public ProjectileMoveType ProjectileMoveType;
    public ProjectileHitType ProjectileHitType;

    [Header("Modifiers")]
    public List<WeaponModifierType> Modifiers;

    [Header("Upgrade Levels")]
    public UpgradeLevel[] UpgradeLevels;

    [Header("Base Effect Chances")]
    public List<WeaponEffectChance> BaseEffectChances;

    [Header("Custom Parameters")]
    public WeaponParameter[] Parameters;

    public T GetParameter<T>(WeaponParameterType type)
    {
        foreach (var p in Parameters)
        {
            if (p.Type == type)
                return p.GetValue<T>();
        }

        Debug.LogWarning($"[{name}] Missing parameter: {type}");
        return default;
    }

}

