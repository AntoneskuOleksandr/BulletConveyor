using UnityEngine;
using Zenject;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    public float damage;
    public Transform firePoint;
    public Transform target;

    protected Bullet _bullet;
    protected BulletsStack _bulletsStack;

    [Inject]
    public void Construct(BulletsStack bulletsStack)
    {
        Debug.Log("Weapon Constructed with BulletsStack");
        _bulletsStack = bulletsStack;
    }

    public virtual void Shoot()
    {
        TryToGetBullet();

        if (_bullet == null)
        {
            Debug.LogError("Bullet is not assigned!");
            return;
        }

        _bullet.Init(damage, firePoint, target);
    }

    protected virtual void TryToGetBullet()
    {
        if (_bulletsStack.HaveBullet)
        {
            _bullet = _bulletsStack.GetBulletFromStack();
        }
        else
        {
            Debug.LogWarning("No bullets available in the stack!");
        }
    }
}
