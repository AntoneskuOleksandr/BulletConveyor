using static EffectChanceManager;

public interface IDebuff
{
    public ProjectileEffectType Type { get; }
    public void Start();
    public void Refresh(IProjectileEffect effect);
}