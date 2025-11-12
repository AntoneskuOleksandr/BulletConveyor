using System.Collections.Generic;

public interface IShootStrategy
{
    bool IsCanShoot(Weapon weapon);
    List<ProjectileBase> Shoot(Weapon weapon);
}
