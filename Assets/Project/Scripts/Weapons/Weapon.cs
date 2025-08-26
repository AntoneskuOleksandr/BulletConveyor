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
    protected EnemyManager _enemyManager;

    [Inject]
    public void Construct(BulletManager bulletManager, EnemyManager enemyManager)
    {
        _bulletManager = bulletManager;
        _enemyManager = enemyManager;
    }

    public virtual void Shoot()
    {
        _bullet = _bulletManager.GetBullet(this);

        if (_bullet == null)
            return; // No available bullets

        FindTarget();

        if (target == null)
            return; // No target found

        _bullet.Init(damage, firePoint, target);
    }

    public virtual void FindTarget()
    {
        target = _enemyManager.GetNearestEnemy(this);
    }
}
