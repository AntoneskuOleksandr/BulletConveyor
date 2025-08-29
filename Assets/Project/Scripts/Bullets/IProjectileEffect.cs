using static EffectChanceManager;

public interface IProjectileEffect
{
    ProjectileEffectType EffectType { get; }
    void Apply(Bullet bullet, Enemy enemy);
}
