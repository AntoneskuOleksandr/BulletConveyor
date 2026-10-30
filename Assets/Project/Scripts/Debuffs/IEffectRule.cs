using System.Collections.Generic;

public interface IEffectRule
{
    void Evaluate(Dictionary<OnHitEffectType, List<IStatusEffect>> effects, ModifierManager manager, EffectRulesConfigSO config);
}

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
                    freeze.StartMelt(2f);
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

public class FreezeRefreshesIceRule : IEffectRule
{
    public void Evaluate(
        Dictionary<OnHitEffectType, List<IStatusEffect>> effects,
        ModifierManager manager,
        EffectRulesConfigSO config)
    {

        if (!effects.TryGetValue(OnHitEffectType.Ice, out var iceList) || iceList.Count == 0)
            return;
        if (!effects.TryGetValue(OnHitEffectType.Freeze, out var freezeList) || freezeList.Count == 0)
            return;

        var freezeToRemove = new List<FreezeDebuff>();
        var iceToRefresh = new List<IceDebuff>();
        float addDuration = 0f;

        foreach (var freezeEffect in freezeList)
        {
            if (freezeEffect is FreezeDebuff freeze)
            {
                freezeToRemove.Add(freeze);

                addDuration = freeze.Duration / 2f;
            }
        }

        foreach (var iceEffect in iceList)
        {
            if (iceEffect is IceDebuff ice)
                iceToRefresh.Add(ice);
        }

        foreach (var freeze in freezeToRemove)
        {
            manager.RemoveStatusEffect(freeze);
        }

        foreach (var ice in iceToRefresh)
        {
            ice.Refresh(new IceDebuff(1f, addDuration));
        }
    }
}

