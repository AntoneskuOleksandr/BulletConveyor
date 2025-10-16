using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeLevel
{
    public int cost;
    public int damageBonus;
    public float fireRateBonus;
    public int healthBonus;
}

public enum WeaponParameterType
{
    AoERadius,
    LaserLength,
    LaserWidth,
    HomingSpeed
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
    public BulletType BulletType;


    [Header("Base Stats")]
    public int baseDamage;
    public float baseFireRate;
    public int baseHealth;
    public float baseAttackRadius;

    [Header("Strategies")]
    public ShootStrategyType shootStrategy;
    public AimStrategyType aimStrategy;
    public TargetStrategyType targetStrategy;
    public BulletMoveType bulletMoveType;
    public BulletHitType bulletHitType;

    [Header("Upgrade Levels")]
    public UpgradeLevel[] upgradeLevels;

    [Header("Base Effect Chances")]
    public List<WeaponEffectChance> baseEffectChances;

    [Header("Custom Parameters")]
    public WeaponParameter[] Parameters;


    public float GetParameter(WeaponParameterType type, float defaultValue = 0f)
    {
        foreach (var p in Parameters)
        {
            if (p.Type == type)
                return p.Value;
        }
        return defaultValue;
    }

}

