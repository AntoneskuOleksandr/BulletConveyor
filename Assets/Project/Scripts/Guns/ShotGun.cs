using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGun : Gun
{
    public int pelletsCount = 5;
    public float spreadAngle = 5f;

    public override void Shoot(GameObject bulletGO)
    {
        for (int i = 0; i < pelletsCount; i++)
        {
            GameObject pellet = bulletGO;
            pellet.transform.position = firePoint.position;
            pellet.transform.rotation = firePoint.rotation;

            Bullet bullet = pellet.GetComponent<Bullet>();
            bullet.GetComponent<Collider>().isTrigger = true;
            bullet.enabled = true;

            if (bullet != null)
            {
                TryGiveEffect(bullet);

                bullet.damage = gunDamage;

                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                bulletRb.isKinematic = false;
                bulletRb.useGravity = false;
                Vector3 direction = target.position - bullet.transform.position;
                direction.y += yEnemyOffSet;

                Quaternion fireRotation = Quaternion.LookRotation(direction);
                Quaternion randomRotation = Random.rotation;
                fireRotation = Quaternion.RotateTowards(fireRotation, randomRotation, Random.Range(0f, spreadAngle));
                bulletRb.linearVelocity = fireRotation * Vector3.forward * bulletSpeed;
            }
        }
    }
}
