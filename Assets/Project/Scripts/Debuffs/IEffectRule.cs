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
    public void Evaluate(
        Dictionary<OnHitEffectType, List<IStatusEffect>> effects,
        ModifierManager manager,
        EffectRulesConfigSO config)
    {
        // Проверяем наличие Ice и Freeze
        if (!effects.TryGetValue(OnHitEffectType.Ice, out var iceList) || iceList.Count == 0)
            return;
        if (!effects.TryGetValue(OnHitEffectType.Freeze, out var freezeList) || freezeList.Count == 0)
            return;

        // Буферы для удаления и обновления
        var freezeToRemove = new List<FreezeDebuff>();
        var iceToRefresh = new List<IceDebuff>();
        float addDuration = 0f;

        // --- Сначала собираем Freeze эффекты, которые нужно удалить
        foreach (var freezeEffect in freezeList)
        {
            if (freezeEffect is FreezeDebuff freeze)
            {
                freezeToRemove.Add(freeze);
                // Допустим, берём длительность первого Freeze — 
                // если нужно комбинировать — можно просуммировать
                addDuration = freeze.Duration / 2f;
            }
        }

        // --- Собираем Ice эффекты, которые будем обновлять
        foreach (var iceEffect in iceList)
        {
            if (iceEffect is IceDebuff ice)
                iceToRefresh.Add(ice);
        }

        // --- Теперь применяем изменения, НЕ во время итерации
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

