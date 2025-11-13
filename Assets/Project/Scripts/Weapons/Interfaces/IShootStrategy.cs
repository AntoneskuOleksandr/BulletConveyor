using System.Collections.Generic;

public interface IShootStrategy
{
    bool IsReadyToRequest(Weapon weapon, int minProjectiles);
    void RequestFire(Weapon weapon, int minProjectiles, int maxProjectiles);
    void ShootAllocated(Weapon weapon, List<ProjectileBase> bullets);
}
