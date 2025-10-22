using UnityEngine;

public class WeaponGizmoDrawer : MonoBehaviour
{
    public Weapon weapon;
    public int steps = 50;
    public float timestep = 0.1f;
    public Color color = Color.yellow;


    private void OnDrawGizmos()
    {
        if (weapon == null)
        {
            weapon = GetComponent<Weapon>();
            return;
        }
        var firePoint = weapon.AttachmentPoints?.Get(WeaponAttachmentType.FirePoint);
        if (firePoint == null) return;

        Vector3 startPos = firePoint.position;
        Vector3 velocity = firePoint.forward * weapon.Data.GetParameter(WeaponParameterType.LaunchSpeed);
        Vector3 gravity = Physics.gravity;

        Vector3 previousPos = startPos;
        Gizmos.color = color;

        for (int i = 1; i <= steps; i++)
        {
            float t = i * timestep;
            Vector3 currentPos = startPos + velocity * t + 0.5f * gravity * t * t;
            Gizmos.DrawLine(previousPos, currentPos);
            previousPos = currentPos;
        }
    }
}
