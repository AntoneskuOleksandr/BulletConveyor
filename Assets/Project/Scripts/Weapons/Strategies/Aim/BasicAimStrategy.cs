using UnityEngine;

public class BasicAimStrategy : IAimStrategy
{
    public void Aim(Weapon weapon)
    {
        var target = weapon.Target;
        if (target == null || weapon.PartToRotate == null)
            return;

        Vector3 dir = target.HitTransform.position - weapon.PartToRotate.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(dir);
        weapon.PartToRotate.rotation = Quaternion.Lerp(
            weapon.PartToRotate.rotation,
            lookRotation,
            Time.deltaTime * weapon.TurnSpeed
        );
    }
}
