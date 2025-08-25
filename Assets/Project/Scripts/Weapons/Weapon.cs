using UnityEngine;
using Zenject;

public enum BulletType
{
    Standard,
    Missile
}

public abstract class Weapon : MonoBehaviour, IWeapon
{
    [SerializeField] private BulletType bulletType;
    public BulletType BulletType => bulletType;
    public float damage;
    public Transform firePoint;
    public Transform target;

    protected Bullet _bullet;
    protected BulletManager _bulletManager;

    [Inject]
    public void Construct(BulletManager bulletManager)
    {
        _bulletManager = bulletManager;
    }

    public virtual void Shoot()
    {
        _bullet = _bulletManager.GetBullet(this);

        if (_bullet == null)
            return; // No available bullets

        _bullet.Init(damage, firePoint, target);
    }
}
