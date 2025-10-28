using UnityEngine;

public class Ballistic : MonoBehaviour
{
    [SerializeField] private Transform target;
    private void Update()
    {
        TryAim();
    }
    public bool TryAim()
    {
        Weapon weapon = GetComponent<Weapon>();

        Transform baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        Transform barrelRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);

        Vector3 firePos = barrelRot.position;
        Vector3 targetPos = target.position;

        float lengthOfBarrel = Vector3.Distance(firePoint.position, barrelRot.position);
        Debug.Log("Length of barrel: " + lengthOfBarrel);


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

        //float highAngleRad = Mathf.Atan((Mathf.Pow(v, 2) + Mathf.Sqrt(discriminant)) / (g * d));
        float lowAngleRad = Mathf.Atan((Mathf.Pow(v, 2) - Mathf.Sqrt(discriminant)) / (g * d));

        //float highAngleDeg = highAngleRad * Mathf.Rad2Deg;
        float lowAngleDeg = lowAngleRad * Mathf.Rad2Deg;

        Debug.Log($"lowAngleDeg: {lowAngleDeg} = Mathf.Atan((Mathf.Pow({v}, {2}) - Mathf.Sqrt({discriminant})) / ({g} * {d}));");

        Quaternion desiredPitch = Quaternion.Euler(-lowAngleDeg, 0f, 0f);
        barrelRot.localRotation = desiredPitch;

        return false;
    }
}