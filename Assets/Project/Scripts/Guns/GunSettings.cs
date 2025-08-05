using UnityEngine;

[CreateAssetMenu(fileName = "GunSettings", menuName = "Scriptable Objects/GunSettings", order = 1)]
public class GunSettings : ScriptableObject
{
    public int ID;
    public float damage;
    public float fireRate;
    public float health;
    public float bulletSpeed;
}
