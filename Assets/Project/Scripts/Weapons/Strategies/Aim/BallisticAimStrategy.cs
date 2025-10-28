using UnityEngine;

public class BallisticAimStrategy : IAimStrategy
{
    private const int MAX_ITERATIONS = 3;
    [SerializeField] private float smoothSpeed = 3f; // скорость сглаживания (серialize для настройки)
    private float currentTheta = 0f; // в радианах

    public bool TryAim(Weapon weapon)
    {
        Transform baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        Transform barrelRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        Transform target = weapon.Target.HitTransform;

        float v = weapon.Data.GetParameter(WeaponParameterType.LaunchSpeed);
        float g = Mathf.Abs(Physics.gravity.y);

        Vector3 t = target.position;
        Vector3 d0 = t - firePoint.position;
        float x0 = new Vector3(d0.x, 0f, d0.z).magnitude;
        float y0 = d0.y;

        float D = Mathf.Pow(v, 4) - g * (g * x0 * x0 + 2 * y0 * v * v);

        // fallback angle (лучший общий выбор) — 45° в радианах
        float fallbackTheta = 45f * Mathf.Deg2Rad;

        float desiredTheta = fallbackTheta;

        if (D < 0f)
        {
            // цель вне досягаемости — используем fallback, но не прерываем плавного движения
            Debug.LogWarning("No ballistic solution found: target out of range.");
        }
        else
        {
            // численное решение: используем currentTheta как стартовое приближение
            float theta = currentTheta;
            float delta = 0.001f;

            for (int i = 0; i < MAX_ITERATIONS; i++)
            {
                barrelRot.localRotation = Quaternion.Euler(-theta * Mathf.Rad2Deg, 0f, 0f);
                Vector3 p = firePoint.position;
                Vector3 d = t - p;
                float x = new Vector3(d.x, 0f, d.z).magnitude;
                float y = d.y;
                float f = x * Mathf.Tan(theta) - (g * x * x) / (2 * v * v * Mathf.Cos(theta) * Mathf.Cos(theta)) - y;

                float th = theta + delta;
                barrelRot.localRotation = Quaternion.Euler(-th * Mathf.Rad2Deg, 0f, 0f);
                Vector3 p2 = firePoint.position;
                Vector3 d2 = t - p2;
                float x2 = new Vector3(d2.x, 0f, d2.z).magnitude;
                float y2 = d2.y;
                float f2 = x2 * Mathf.Tan(th) - (g * x2 * x2) / (2 * v * v * Mathf.Cos(th) * Mathf.Cos(th)) - y2;

                theta -= f * (delta) / (f2 - f);
            }

            desiredTheta = theta;
        }

        // Плавно интерполируем текущий угол к желаемому (в градусах для LerpAngle)
        float currentDeg = currentTheta * Mathf.Rad2Deg;
        float desiredDeg = desiredTheta * Mathf.Rad2Deg;
        float newDeg = Mathf.LerpAngle(currentDeg, desiredDeg, Time.deltaTime * smoothSpeed);
        currentTheta = newDeg * Mathf.Deg2Rad;

        // Применяем итоговый сглаженный угол
        barrelRot.localRotation = Quaternion.Euler(-newDeg, 0f, 0f);

        Debug.Log($"Aiming at target. Desired angle: {desiredTheta * Mathf.Rad2Deg}°, Current angle: {newDeg}°");

        return true;
    }
}
