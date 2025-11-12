public interface IProjectileMoveStrategy
{
    void Launch(Weapon weapon, ProjectileBase projectile, ProjectileInitData data);
    void Update(Weapon weapon, ProjectileBase projectile, float deltaTime);
}
