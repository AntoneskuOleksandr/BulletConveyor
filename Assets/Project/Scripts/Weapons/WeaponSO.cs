using UnityEngine;

[System.Serializable]
public class UpgradeLevel
{
    public int cost;          // цена на этом уровне
    public int damageBonus;
    public float fireRateBonus;
    public int healthBonus;
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponSO : ScriptableObject
{
    public int WeaponId;
    public string WeaponName;
    public GameObject WeaponGO;

    [Header("Base Stats")]
    public int baseDamage;
    public float baseFireRate;
    public int baseHealth;

    [Header("Upgrades")]
    public UpgradeLevel[] upgradeLevels;
}
