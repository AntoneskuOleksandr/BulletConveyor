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
    AoERadius,
    PelletCount,
    SpreadAngle,
    PowerfulShotEvery,
    PowerShotMultiplier
}

[Serializable]
public class WeaponParameter
{
    public WeaponParameterType Type;
    public float Value;
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
    public float YawTurnSpeed;
    public float PitchTurnSpeed;
    public bool UseYaw;
    public bool UsePitch;

    [Header("Bullet Settings")]
    public LaunchSpeedMode LaunchSpeedMode = LaunchSpeedMode.Fixed;
    public float LaunchSpeed = 10f;

    [Header("Strategies")]
    public ShootStrategyType ShootStrategy;
    public List<AimStrategyType> AimStrategies;
    public TargetStrategyType TargetStrategy;
    public BulletMoveType BulletMoveType;
    public BulletHitType BulletHitType;

    [Header("Upgrade Levels")]
    public UpgradeLevel[] UpgradeLevels;

    [Header("Base Effect Chances")]
    public List<WeaponEffectChance> BaseEffectChances;

    [Header("Custom Parameters")]
    public WeaponParameter[] Parameters;

    public float GetParameter(WeaponParameterType type, float defaultValue = 0f)
    {
        foreach (var p in Parameters)
        {
            if (p.Type == type)
                return p.Value;
        }

        Debug.LogWarning($"Parameter of type {type} not found in weapon {name}. Returning default value {defaultValue}.");
        return defaultValue;
    }
}

