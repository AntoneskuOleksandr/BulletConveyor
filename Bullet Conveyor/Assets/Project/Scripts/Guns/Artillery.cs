using UnityEngine;

public class Artillery : Gun
{
    public ArtilleryShell shellPrefab;
    public float angleInDegrees;
    public float minRange;

    public override void Shoot(GameObject bulletGO)
    {
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            ArtilleryShell shell = Instantiate(shellPrefab, firePoint.position, firePoint.rotation);

            TryGiveEffect(shell);

            shell.damage = gunDamage;

            Destroy(bulletGO);

            shell.enabled = true;
            shell.GetComponent<Collider>().isTrigger = true;

            Rigidbody shellRb = shell.GetComponent<Rigidbody>();
            shellRb.isKinematic = false;

            Vector3 targetDir = target.position - firePoint.position;
            float h = targetDir.y;
            targetDir.y = 0;
            float dist = targetDir.magnitude;
            float a = angleInDegrees * Mathf.Deg2Rad;
            targetDir.y = dist * Mathf.Tan(a);
            dist += h / Mathf.Tan(a);

            if (dist <= 0)
            {
                return;
            }

            float vel = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
            shellRb.linearVelocity = vel * targetDir.normalized;
        }
    }
}
