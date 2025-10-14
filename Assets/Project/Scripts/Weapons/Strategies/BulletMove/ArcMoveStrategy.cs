using UnityEngine;

public class ArcMoveStrategy : IBulletMoveStrategy
{
    public void Launch(Bullet bullet, Transform target)
    {
        Vector3 dir = target.position - bullet.transform.position;
        bullet.Rigidbody.useGravity = true;

        float distance = dir.magnitude;
        float angle = 45f * Mathf.Deg2Rad;
        float v = Mathf.Sqrt(distance * Physics.gravity.magnitude / Mathf.Sin(2 * angle));

        Vector3 velocity = dir.normalized * v * Mathf.Cos(angle) + Vector3.up * v * Mathf.Sin(angle);
        bullet.Rigidbody.linearVelocity = velocity;
    }
}
