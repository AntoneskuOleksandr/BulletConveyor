using System.Collections.Generic;

public interface IEffectRule
{
    void Evaluate(Dictionary<OnHitEffectType, IStatusEffect> effects, ModifierManager manager);
}

// Freeze → Ice
public class FreezeToIceRule : IEffectRule
{
    public void Evaluate(Dictionary<OnHitEffectType, IStatusEffect> effects, ModifierManager manager)
    {
        if (effects.TryGetValue(OnHitEffectType.Freeze, out var effect)
            && effect is FreezeDebuff freeze
            && freeze.Strength >= 1f
            && !manager.HasStatusEffect(OnHitEffectType.Ice))
        {
            manager.RemoveStatusEffect(OnHitEffectType.Freeze);
            manager.AddStatusEffect(new IceDebuff(2f));
        }
    }
}

// Fire ускоряет таяние Freeze
public class FireMeltsFreezeRule : IEffectRule
{
    public void Evaluate(Dictionary<OnHitEffectType, IStatusEffect> effects, ModifierManager manager)
    {
        if (effects.TryGetValue(OnHitEffectType.Freeze, out var freezeEffect)
            && freezeEffect is FreezeDebuff freeze
            && effects.TryGetValue(OnHitEffectType.Fire, out var fireEffect)
            && fireEffect is ITickEffect fire)
        {
            freeze.StartMelt(2f); // можно сделать зависимость от DPS
        }
    }
}

public class FireMeltsIceRule : IEffectRule
{
    public void Evaluate(Dictionary<OnHitEffectType, IStatusEffect> effects, ModifierManager manager)
    {
        if (effects.TryGetValue(OnHitEffectType.Ice, out var iceEffect)
            && iceEffect is IceDebuff ice
            && effects.TryGetValue(OnHitEffectType.Fire, out var fireEffect)
            && fireEffect is ITickEffect fire)
        {
            ice.StartMelt(2f);
        }
    }
}

// Если враг заморожен (Ice), то Freeze продлевает длительность Ice
public class FreezeRefreshesIceRule : IEffectRule
{
    public void Evaluate(Dictionary<OnHitEffectType, IStatusEffect> effects, ModifierManager manager)
    {
        if (effects.TryGetValue(OnHitEffectType.Ice, out var iceEffect)
            && iceEffect is IceDebuff ice
            && effects.TryGetValue(OnHitEffectType.Freeze, out var freezeEffect)
            && freezeEffect is FreezeDebuff freeze)
        {
            // Убираем Freeze (он не должен висеть одновременно с Ice)
            manager.RemoveStatusEffect(OnHitEffectType.Freeze);

            // Продлеваем Ice на половину от Duration Freeze
            ice.Refresh(new IceDebuff(freeze.Duration / 2f));
        }
    }
}

