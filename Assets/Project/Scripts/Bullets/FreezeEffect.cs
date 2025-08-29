public class FreezeEffect : IOnHitEffect
{
    public OnHitEffectType EffectType => OnHitEffectType.Freeze;

    private float _slowAmount;    // Например, 0.5 ? половина скорости
    private float _duration;      // Время действия "замедления"
    private float _stackIncrement; // Как сильно добавляется замедление при повторном попадании

    public FreezeEffect(float slowAmount = 0.3f, float duration = 2f, float stackIncrement = 0.1f)
    {
        _slowAmount = slowAmount;
        _duration = duration;
        _stackIncrement = stackIncrement;
    }

    public void Apply(Bullet bullet, Enemy target)
    {
        var debuff = new FreezeDebuff(_slowAmount, _duration, _stackIncrement);
        target.AddStatusEffect(debuff);
    }
}
