using UnityEngine;

public class BallisticAimStrategy : IAimStrategy
{
    private const int MAX_ITERATIONS = 3;
    public bool TryAim(Weapon weapon)
    {
        Transform baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        Transform barrelRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        Transform target = weapon.Target.transform;

        float v = weapon.Data.GetParameter(WeaponParameterType.LaunchSpeed);
        float g = Mathf.Abs(Physics.gravity.y);

        // начальное приближение угла
        float theta = 0f;
        float delta = 0.001f;

        for (int i = 0; i < MAX_ITERATIONS; i++)
        {
            // вычисляем FirePoint для текущего угла
            barrelRot.localRotation = Quaternion.Euler(-theta * Mathf.Rad2Deg, 0f, 0f);
            Vector3 p = firePoint.position;
            Vector3 t = target.position;
            Vector3 d = t - p;
            float x = new Vector3(d.x, 0f, d.z).magnitude;
            float y = d.y;

            float f = x * Mathf.Tan(theta) - (g * x * x) / (2 * v * v * Mathf.Cos(theta) * Mathf.Cos(theta)) - y;

            float f2 = 0f;
            {
                float th = theta + delta;
                barrelRot.localRotation = Quaternion.Euler(-th * Mathf.Rad2Deg, 0f, 0f);
                Vector3 p2 = firePoint.position;
                Vector3 d2 = t - p2;
                float x2 = new Vector3(d2.x, 0f, d2.z).magnitude;
                float y2 = d2.y;
                f2 = x2 * Mathf.Tan(th) - (g * x2 * x2) / (2 * v * v * Mathf.Cos(th) * Mathf.Cos(th)) - y2;
            }

            theta -= f * (delta) / (f2 - f);
        }

        barrelRot.localRotation = Quaternion.Euler(-theta * Mathf.Rad2Deg, 0f, 0f);
        return false;
    }
}
