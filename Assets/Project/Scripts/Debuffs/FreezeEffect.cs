public class FreezeEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;

    private float _duration;      // Время действия "замедления"
    private float _slowAmount;    // Например, 0.5 ? половина скорости

    public FreezeEffect(float duration = 2f, float slowAmount = 0.3f)
    {
        _slowAmount = slowAmount;
        _duration = duration;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        var debuff = new FreezeDebuff(_duration, _slowAmount);
        target.AddStatusEffect(debuff);
    }
}
