public class IceEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Ice;

    private float _meltSpeed;
    private float _duration;

    public IceEffect(float meltSpeed, float duration)
    {
        _meltSpeed = meltSpeed;
        _duration = duration;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        IceDebuff debuff = new IceDebuff(_meltSpeed, _duration);
        target.ModifierManager.AddStatusEffect(debuff);
    }
}
