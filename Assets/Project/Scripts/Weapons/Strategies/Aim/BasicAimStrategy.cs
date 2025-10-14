using UnityEngine;

public class BasicAimStrategy : IAimStrategy
{
    public bool TryAim(Weapon weapon)
    {
        var target = weapon.Target;
        if (target == null || weapon.PartToRotate == null)
            return false;

        // Вектор на цель
        Vector3 dir = target.HitTransform.position - weapon.PartToRotate.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return false;

        // Текущий и целевой поворот
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        weapon.PartToRotate.rotation = Quaternion.Lerp(
            weapon.PartToRotate.rotation,
            lookRotation,
            Time.deltaTime * weapon.TurnSpeed
        );

        // --- Проверяем угол ---
        float angle = Quaternion.Angle(weapon.PartToRotate.rotation, lookRotation);
        return angle <= weapon.AimThreshold;
    }
}
