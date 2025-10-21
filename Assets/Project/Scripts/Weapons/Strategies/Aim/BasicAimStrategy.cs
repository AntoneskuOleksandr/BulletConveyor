using UnityEngine;

public class BasicAimStrategy : IAimStrategy
{
    public bool TryAim(Weapon weapon)
    {
        var target = weapon.Target;

        // Вектор на цель
        Vector3 dir = target.HitTransform.position - weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation).position;
        dir.y = 0f;
        if (dir == Vector3.zero) return false;

        // Текущий и целевой поворот
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation).rotation = Quaternion.Lerp(
            weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation).rotation,
            lookRotation,
            Time.deltaTime * weapon.TurnSpeed
        );

        // --- Проверяем угол ---
        float angle = Quaternion.Angle(weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation).rotation, lookRotation);
        return angle <= weapon.AimThreshold;
    }
}
