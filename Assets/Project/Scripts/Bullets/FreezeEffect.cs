public class FreezeEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;

    private float _slowAmount;    // Например, 0.5 ? половина скорости
    private float _duration;      // Время действия "замедления"

    public FreezeEffect(float slowAmount = 0.3f, float duration = 2f)
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
