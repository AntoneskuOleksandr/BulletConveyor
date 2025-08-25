using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    public float damage;
    public abstract void Shoot();
}
