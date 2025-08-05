using UnityEngine;

public class MachineGun : Gun
{
    public override void Shoot(GameObject bulletGO)
    {
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet == null)
            Debug.LogError("There is no bullet");

        bulletGO.transform.position = firePoint.position;
        bulletGO.transform.rotation = firePoint.rotation;

        bullet.enabled = true;
        bulletGO.GetComponent<Collider>().isTrigger = true;

        TryGiveEffect(bullet);

        bullet.damage = gunDamage;

        bulletRb = bulletGO.GetComponent<Rigidbody>();
        bulletRb.isKinematic = false;
        bulletRb.useGravity = false;

        direction = target.position - bullet.transform.position;
        direction.y += yEnemyOffSet;

        bulletRb.linearVelocity = direction.normalized * bulletSpeed;

        //audioManager.PlaySFX(audioManager.machineGunShot);
    }
}
