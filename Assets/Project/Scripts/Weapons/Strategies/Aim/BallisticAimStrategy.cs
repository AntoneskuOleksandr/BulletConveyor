using System;
using UnityEngine;

public class BallisticAimStrategy : IAimStrategy
{
    public bool TryAim(Weapon weapon)
    {
        if (weapon == null || weapon.Target == null || weapon.AttachmentPoints == null)
            return false;

        var firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        var baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        var barrelRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);

        if (firePoint == null) return false;

        // параметры
        float g = Mathf.Abs(Physics.gravity.y);
        Vector3 origin = firePoint.position;
        Vector3 targetPos = weapon.Target.HitTransform.position;

        // горизонтальная дистанция по XZ (R)
        Vector3 diff = targetPos - origin;
        diff.y = 0f;
        float R = diff.magnitude;

        float v = weapon.Data?.GetParameter(WeaponParameterType.LaunchSpeed, 0f) ?? 0f;
        if (v <= 0f)
        {
            Debug.LogWarning("[BallisticAim] launch speed <= 0");
            return false;
        }

        // значение под asin и флаг достижимости
        float rawValue = (R * g) / (v * v);
        bool canHit = rawValue <= 1f;

        // для вычисления угла безопасно ограничиваем значение в [-1,1]
        float clamped = Mathf.Clamp(rawValue, -1f, 1f);
        float angleRadLow = 0.5f * Mathf.Asin(clamped); // низкий угол
        float angleDegLow = angleRadLow * Mathf.Rad2Deg;

        Debug.Log($"[BallisticAim] canHit={canHit} | angle={angleDegLow:F2}° | R={R:F2} | v={v:F2} | raw={rawValue:F3}");

        // желаемое направление: возьмём горизонтальную нормаль и поднимем на angleRadLow
        Vector3 horizontalDir = (R > 0f) ? diff.normalized : firePoint.forward;
        Vector3 desiredDir = Mathf.Cos(angleRadLow) * horizontalDir + Mathf.Sin(angleRadLow) * Vector3.up;
        desiredDir.Normalize();

        Quaternion desiredRot = Quaternion.LookRotation(desiredDir, Vector3.up);

        // плавный поворот как в BasicAimStrategy (используем weapon.TurnSpeed)
        ApplyRotationLerp(desiredRot, baseRot, barrelRot, weapon.TurnSpeed);

        // проверяем наведение (угловая погрешность)
        bool oriented = IsAimed(desiredRot, baseRot, barrelRot, weapon.AimThreshold);

        if (!canHit)
        {
            // цель вне максимальной дальности при данной скорости — смотрим в "максимально дальнобойный" угол,
            // но не разрешаем стрелять (TryAim возвращает false)
            return false;
        }

        // можно стрелять только если цель достижима и оружие выровнено
        return oriented;
    }

    private void ApplyRotationLerp(Quaternion desiredRot, Transform baseRot, Transform barrelRot, float turnSpeed)
    {
        if (baseRot != null)
        {
            Vector3 desiredEuler = desiredRot.eulerAngles;
            Vector3 currentEuler = baseRot.rotation.eulerAngles;
            Quaternion targetY = Quaternion.Euler(currentEuler.x, desiredEuler.y, currentEuler.z);
            baseRot.rotation = Quaternion.Lerp(baseRot.rotation, targetY, Time.deltaTime * turnSpeed);
        }

        if (barrelRot != null && baseRot != null)
        {
            Quaternion localDesired = Quaternion.Inverse(baseRot.rotation) * desiredRot;
            Vector3 localEuler = localDesired.eulerAngles;
            float pitch = NormalizeAngle(localEuler.x);

            Vector3 currentLocal = barrelRot.localEulerAngles;
            float newPitch = Mathf.LerpAngle(currentLocal.x, pitch, Time.deltaTime * turnSpeed);
            barrelRot.localEulerAngles = new Vector3(newPitch, currentLocal.y, currentLocal.z);
        }
    }

    private bool IsAimed(Quaternion desired, Transform baseRot, Transform barrelRot, float threshold)
    {
        if (baseRot == null) return false;

        Vector3 currentForward = (barrelRot != null) ? barrelRot.forward : baseRot.forward;
        Vector3 desiredFwd = desired * Vector3.forward;
        float angle = Vector3.Angle(currentForward, desiredFwd);
        return angle <= threshold;
    }

    private float NormalizeAngle(float a)
    {
        a = (a + 180f) % 360f - 180f;
        return a;
    }
}
