using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinsGun : Gun
{
    public Transform firePoint1;
    public Transform firePoint2;
    private bool lastFiredFirst = false;

    public override void Shoot(GameObject bulletGO)
    {
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        firePoint = lastFiredFirst ? firePoint2 : firePoint1;

        lastFiredFirst = !lastFiredFirst;

        bulletGO.transform.position = firePoint.position;
        bulletGO.transform.rotation = firePoint.rotation;

        bullet.enabled = true;
        bulletGO.GetComponent<Collider>().isTrigger = true;

        if (bullet != null)
        {
            TryGiveEffect(bullet);

            bullet.damage = gunDamage;

            bulletRb = bulletGO.GetComponent<Rigidbody>();
            bulletRb.isKinematic = false;
            bulletRb.useGravity = false;

            direction = target.position - bullet.transform.position;
            direction.y += yEnemyOffSet;

            bulletRb.linearVelocity = direction.normalized * bulletSpeed;
        }
    }
}
