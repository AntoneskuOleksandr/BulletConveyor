using System.Collections.Generic;

public interface IEffectRule
{
    void Evaluate(Dictionary<OnHitEffectType, List<IStatusEffect>> effects, ModifierManager manager, EffectRulesConfigSO config);
}

// Freeze → Ice
public class FreezeToIceRule : IEffectRule
{
    public void Evaluate(Dictionary<OnHitEffectType, List<IStatusEffect>> effects, ModifierManager manager, EffectRulesConfigSO config)
    {
        if (effects.TryGetValue(OnHitEffectType.Freeze, out var freezeList) && freezeList.Count > 0)
        {
            foreach (var effect in freezeList)
            {
                if (effect is FreezeDebuff freeze && freeze.Strength >= 1f && !manager.HasStatusEffect(OnHitEffectType.Ice))
                {
                    manager.RemoveStatusEffect(effect);
                    manager.AddStatusEffect(new IceDebuff(1f, 2f));
                }
            }
        }
    }
}

// Fire ускоряет таяние Freeze
public class FireMeltsFreezeRule : IEffectRule
{
    public void Evaluate(Dictionary<OnHitEffectType, List<IStatusEffect>> effects, ModifierManager manager, EffectRulesConfigSO config)
    {
        if (effects.TryGetValue(OnHitEffectType.Freeze, out var freezeList) && freezeList.Count > 0
            && effects.TryGetValue(OnHitEffectType.Fire, out var fireList) && fireList.Count > 0)
        {
            foreach (var effect in freezeList)
            {
                if (effect is FreezeDebuff freeze)
                {
                    freeze.StartMelt(2f); // можно сделать зависимость от DPS
                }
            }
        }
    }
}

public class FireMeltsIceRule : IEffectRule
{
    public void Evaluate(Dictionary<OnHitEffectType, List<IStatusEffect>> effects, ModifierManager manager, EffectRulesConfigSO config)
    {
        if (effects.TryGetValue(OnHitEffectType.Ice, out var iceList) && iceList.Count > 0
            && effects.TryGetValue(OnHitEffectType.Fire, out var fireList) && fireList.Count > 0)
        {
            foreach (var effect in iceList)
            {
                if (effect is IceDebuff ice)
                {
                    ice.StartMelt(2f);
                }
            }
        }
    }
}

// Если враг заморожен (Ice), то Freeze продлевает длительность Ice
public class FreezeRefreshesIceRule : IEffectRule
{
    public void Evaluate(Dictionary<OnHitEffectType, List<IStatusEffect>> effects, ModifierManager manager, EffectRulesConfigSO config)
    {
        if (effects.TryGetValue(OnHitEffectType.Ice, out var iceList) && iceList.Count > 0
            && effects.TryGetValue(OnHitEffectType.Freeze, out var freezeList) && freezeList.Count > 0)
        {
            foreach (var freezeEffect in freezeList)
            {
                if (freezeEffect is FreezeDebuff freeze)
                {
                    // Убираем Freeze
                    manager.RemoveStatusEffect(freeze);

                    // Продлеваем каждый Ice на половину от Duration Freeze
                    foreach (var iceEffect in iceList)
                    {
                        if (iceEffect is IceDebuff ice)
                        {
                            ice.Refresh(new IceDebuff(1f, freeze.Duration / 2f));
                        }
                    }
                }
            }
        }
    }
}
