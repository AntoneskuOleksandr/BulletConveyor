using UnityEngine;

public class Ballistic : MonoBehaviour
{
    [SerializeField] private Transform target;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TryAim();
    }
    public bool TryAim()
    {
        Weapon weapon = GetComponent<Weapon>();

        Transform baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        Transform barrelRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);
        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);

        Vector3 firePos = firePoint.position;

        Vector3 targetPos = target.position;

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
        float x = toTargetXZ.magnitude;

        // Height diff
        float y = targetPos.y - firePos.y;

        float g = Mathf.Abs(Physics.gravity.y);

        float discriminant = Mathf.Pow(v, 4) - g * (g * Mathf.Pow(x, 2) + 2 * y * Mathf.Pow(v, 2));

        Debug.Log($"discriminant: {discriminant} = Mathf.Pow({v}, {4}) - {g} * ({g} * Mathf.Pow({x}, {2}) + 2 * {y} * Mathf.Pow({v}, {2}))");

        float lowAngleRad = Mathf.Atan((Mathf.Pow(v, 2) - Mathf.Sqrt(discriminant)) / (g * x));

        float lowAngleDeg = lowAngleRad * Mathf.Rad2Deg;
        Debug.Log($"lowAngleDeg: {lowAngleDeg} = Mathf.Atan((Mathf.Pow({v}, {2}) - Mathf.Sqrt({discriminant})) / ({g} * {x}));");

        Debug.Log($"v: {v},\n" +
            $"x: {x},\n" +
            $"y: {y},\n" +
            $"g: {g},\n" +
            $"discriminant: {discriminant}");

        Quaternion desiredPitch = Quaternion.Euler(-lowAngleDeg, 0f, 0f);
        barrelRot.localRotation = desiredPitch;

        return false;
    }
}