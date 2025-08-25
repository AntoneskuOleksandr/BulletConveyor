using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    public float damage;
    public Bullet bullet;
    public Transform firePoint;

    public Transform target;
    public virtual void Shoot()
    {
        if (bullet == null)
        {
            Debug.LogError("Bullet is not assigned!");
            return;
        }

        bullet.Init(damage, firePoint, target);
    }
}
