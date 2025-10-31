using UnityEngine;
using Zenject;

public class BulletManager
{
    [Inject] private BulletsStack _bulletsStack;

    public int AvailableCount => _bulletsStack.bullets.Count;

    public Bullet GetBullet()
    {
        if (!_bulletsStack.HaveBullet)
        {
            Debug.LogError($"No bullets available in the stack");
            return null;
        }

        GameObject bulletGO = _bulletsStack.GetBulletGOFromStack();
        if (bulletGO == null)
        {
            Debug.LogError($"Failed to get bullet GameObject from stack");
            return null;
        }

        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet == null)
            bullet = bulletGO.AddComponent<Bullet>();

        return bullet;
    }
}
