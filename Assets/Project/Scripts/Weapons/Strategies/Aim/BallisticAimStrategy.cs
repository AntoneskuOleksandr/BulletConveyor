using UnityEngine;

public class BallisticAimStrategy : IAimStrategy
{
    public bool TryAim(Weapon weapon)
    {
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        Transform baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        Transform barrelRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);

        Vector3 firePos = firePoint.position;
        Vector3 targetPos = weapon.Target.HitTransform.position;


        if (firePoint == null || baseRot == null || barrelRot == null)
        {
            Debug.LogError("One of attachment transforms is missing (FirePoint / BaseRotation / BarrelRotation).");
            return false;
        }

        // Speed (m/s)
        float v = weapon.Data.GetParameter(WeaponParameterType.LaunchSpeed);

        Vector3 toTarget = targetPos - firePos;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        // Distance to target (XZ)
        float d = toTargetXZ.magnitude;

        // Height diff
        float y = targetPos.y - firePos.y;

        float g = Mathf.Abs(Physics.gravity.y);

        float discriminant = Mathf.Pow(v, 4) - g * (g * Mathf.Pow(d, 2) + 2 * y * Mathf.Pow(v, 2));


        float highAngleRad = Mathf.Atan((Mathf.Pow(v, 2) + Mathf.Sqrt(discriminant)) / (g * d));
        float lowAngleRad = Mathf.Atan((Mathf.Pow(v, 2) - Mathf.Sqrt(discriminant)) / (g * d));

        Quaternion desiredLocalPitch = Quaternion.Euler(-lowAngleRad * Mathf.Rad2Deg, 0f, 0f);
        barrelRot.localRotation = desiredLocalPitch;

        Debug.Log("BarrelRot: " + barrelRot);

        return false;
    }
}
