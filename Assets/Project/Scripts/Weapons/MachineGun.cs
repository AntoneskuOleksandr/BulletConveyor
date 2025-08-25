using UnityEngine;

public class MachineGun : Weapon
{
    public override void Shoot()
    {
        base.Shoot();

        if (bullet == null)
            Debug.LogError("There is no bullet");
    }
}
