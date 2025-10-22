using System;
using UnityEngine;

public class BallisticAimStrategy : IAimStrategy
{
    public bool TryAim(Weapon weapon)
    {
        Vector3 firePos = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint).position;
        Vector3 targetPos = weapon.Target.HitTransform.position;
        Vector3 toTarget = targetPos - firePos;

        // Горизонтальная проекция вектора до цели (по XZ)
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float horizontalDistance = toTargetXZ.magnitude; // d

        // Вертикальная разница высот (цель относительно firePoint)
        float verticalDifference = toTarget.y; // y

        // Скорость и её степени
        float v = weapon.Data.GetParameter(WeaponParameterType.LaunchSpeed);
        float v2 = v * v;
        float v4 = v2 * v2;
        float g = Mathf.Abs(Physics.gravity.y);

        // Логируем исходные данные
        Debug.LogFormat("[BallisticAngleLogger] firePos={0}, targetPos={1}", firePos, targetPos);
        Debug.LogFormat("[BallisticAngleLogger] horizontalDistance(d) = {0:F4} m, verticalDifference(y) = {1:F4} m", horizontalDistance, verticalDifference);
        Debug.LogFormat("[BallisticAngleLogger] launchSpeed(v) = {0:F4} m/s, gravity(g) = {1:F4} m/s²", v, g);

        // 2) Вычисляем дискриминант:
        // Delta = v^4 - g * (g * d^2 + 2 * y * v^2)

        float inside = g * g * horizontalDistance * horizontalDistance + 2f * verticalDifference * v2;
        float discriminant = v4 - inside;

        Debug.LogFormat("[BallisticAngleLogger] discriminant (Δ) = {0}", discriminant);

        if (discriminant < 0.0)
        {
            Debug.LogWarning("[BallisticAngleLogger] No real ballistic solution: the projectile with this speed cannot reach the target (Δ < 0).");
        }

        // 3) Корень и знаменатель
        float sqrtDelta = Mathf.Sqrt(discriminant);
        float denom = g * horizontalDistance;

        // защищённое вычисление тангенсов (они могут быть большими)
        float tanThetaHigh = (v2 + sqrtDelta) / denom;
        float tanThetaLow = (v2 - sqrtDelta) / denom;

        // 4) Углы в радианах
        float thetaHighRad = Mathf.Atan(tanThetaHigh);
        float thetaLowRad = Mathf.Atan(tanThetaLow);

        // 5) Перевод в градусы для удобства чтения
        float thetaHighDeg = (float)(thetaHighRad * (180.0 / Math.PI));
        float thetaLowDeg = (float)(thetaLowRad * (180.0 / Math.PI));

        Debug.LogFormat("[BallisticAngleLogger] Two solutions found:\n - High arc angle = {0:F4}° (≈ {1:F4} rad)\n - Low  arc angle = {2:F4}° (≈ {3:F4} rad)",
            thetaHighDeg, thetaHighRad, thetaLowDeg, thetaLowRad);

        // Вспомогательная информация: время полёта для выбранных углов (полезно для проверки/предсказания)
        // t = d / (v * cos(theta))
        float cosHigh = Mathf.Cos(thetaHighRad);
        float cosLow = Mathf.Cos(thetaLowRad);

        if (Mathf.Abs(cosHigh) > 1e-9)
        {
            float timeHigh = horizontalDistance / (v * cosHigh);
            Debug.LogFormat("[BallisticAngleLogger] Estimated flight time (high arc) = {0:F4} s", timeHigh);
        }

        if (Mathf.Abs(cosLow) > 1e-9)
        {
            float timeLow = horizontalDistance / (v * cosLow);
            Debug.LogFormat("[BallisticAngleLogger] Estimated flight time (low arc)  = {0:F4} s", timeLow);
        }

        return false;
    }
}
