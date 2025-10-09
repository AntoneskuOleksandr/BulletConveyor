using System;
using UnityEngine;

[Serializable]
public class UpgradeLevel
{
    public int cost;
    public int damageBonus;
    public float fireRateBonus;
    public int healthBonus;
}

public enum WeaponType
{
    MachineGun
}

[Serializable]
public class WeaponParameter
{
    public string Name;
    public float Value;
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("General Info")]
    public int WeaponId;
    public string WeaponName;
    public GameObject WeaponGO;
    public WeaponType WeaponType;

    [Header("Base Stats")]
    public int baseDamage;
    public float baseFireRate;
    public int baseHealth;

    [Header("Upgrade Levels")]
    public UpgradeLevel[] upgradeLevels;

    [Header("Custom Parameters")]
    public WeaponParameter[] Parameters;

    public float GetParameter(string name, float defaultValue = 0f)
    {
        foreach (var p in Parameters)
        {
            if (p.Name == name)
                return p.Value;
        }
        return defaultValue;
    }
}

