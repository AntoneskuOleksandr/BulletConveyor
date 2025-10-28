using UnityEngine;

public class BallisticAimStrategy : IAimStrategy
{
    private const int MAX_ITERATIONS = 3;
    private float currentThetaDeg;
    private bool initialized = false;

    public bool TryAim(Weapon weapon)
    {
        Transform baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        Transform barrelRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        Transform target = weapon.Target.HitTransform;

        if (!baseRot || !barrelRot || !firePoint || !target)
            return false;

        if (!initialized)
        {
            currentThetaDeg = Mathf.DeltaAngle(0f, barrelRot.localEulerAngles.x);
            initialized = true;
        }

        float g = Mathf.Abs(Physics.gravity.y);
        float v = weapon.LaunchSpeed;


        Vector3 t = target.position;
        Vector3 d0 = t - firePoint.position;
        float x0 = new Vector3(d0.x, 0f, d0.z).magnitude;
        float y0 = d0.y;

        float D = Mathf.Pow(v, 4) - g * (g * x0 * x0 + 2 * y0 * v * v);
        if (D < 0f)
            return false;

        float theta = 0f;
        float delta = 0.001f;

        for (int i = 0; i < MAX_ITERATIONS; i++)
        {
            float f = x0 * Mathf.Tan(theta) - (g * x0 * x0) / (2 * v * v * Mathf.Cos(theta) * Mathf.Cos(theta)) - y0;
            float f2 = x0 * Mathf.Tan(theta + delta) - (g * x0 * x0) / (2 * v * v * Mathf.Cos(theta + delta) * Mathf.Cos(theta + delta)) - y0;
            theta -= f * delta / (f2 - f);
        }

        float targetThetaDeg = -theta * Mathf.Rad2Deg;

        currentThetaDeg = Mathf.MoveTowardsAngle(
            currentThetaDeg,
            targetThetaDeg,
            weapon.Data.GetParameter(WeaponParameterType.RotationSpeed) * Time.deltaTime
        );

        barrelRot.localRotation = Quaternion.Euler(currentThetaDeg, 0f, 0f);

        return true;
    }
}
