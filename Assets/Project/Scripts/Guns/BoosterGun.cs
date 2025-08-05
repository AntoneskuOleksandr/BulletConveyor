using UnityEngine;

public class BoosterGun : Gun
{
    private Transform lastTarget;
    public float fireRateIncrease = 0.1f;
    public float maxFireRate = 10f;
    private float defaultFireRate;

    public override void Start()
    {
        base.Start();

        defaultFireRate = fireRate;
    }

    public override void Shoot(GameObject bulletGO)
    {
        Bullet bullet = bulletGO.GetComponent<Bullet>();
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

    public override void UpdateTarget()
    {
        base.UpdateTarget();

        if (target == lastTarget)
        {
            fireRate = Mathf.Min(fireRate + fireRateIncrease, maxFireRate);
        }
        else
        {
            fireRate = defaultFireRate;
        }
        lastTarget = target;
    }
}
