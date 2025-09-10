using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ƒебафф молнии: при первом тикe наносит указанный урон (однократно)
/// и даЄт посто€нное замедление _slow на прот€жении duration.
/// –еализован как Refreshable: при повторном наложении обновл€ет duration и берЄт максимум slow.
/// </summary>
public class LightningDebuff : BaseStatusEffect, ITickableEffect, IModifierEffect
{
    public override OnHitEffectType EffectType => OnHitEffectType.Lightning;
    public override EffectStackType StackType => EffectStackType.Refreshable;

    private float _damage;
    private float _slow;
    private bool _damageApplied;

    public LightningDebuff(float damage, float slowAmount, float duration)
    {
        _damage = Mathf.Max(0f, damage);
        _slow = Mathf.Clamp01(slowAmount);
        Duration = Mathf.Max(0.01f, duration);
        Elapsed = 0f;
        _damageApplied = false;
    }

    // Tick вызываетс€ менеджером эффектов каждый кадр: даЄм урон один раз и поддерживаем замедление
    public void Tick(float deltaTime, Enemy enemy)
    {
        if (enemy == null || IsFinished) return;

        // первый тик Ч нанесЄм урон сразу
        if (!_damageApplied)
        {
            if (_damage > 0f)
                enemy.TakeDamage(_damage);
            _damageApplied = true;
        }

        // увеличиваем врем€
        Elapsed += deltaTime;

        // примен€ем замедление напр€мую (как у FreezeDebuff Ч чтобы гарантированно было)
        // если в твоей системе модификаторы складываютс€ централизованно, можно убрать SetSpeed
        enemy.SetSpeed(enemy.BaseSpeed * (1f - _slow));
    }

    public override void Refresh(IStatusEffect newEffect)
    {
        if (newEffect is LightningDebuff ld)
        {
            // продлеваем наибольшую длительность, берЄм максимальное замедление
            Duration = Mathf.Max(Duration, ld.Duration);
            _slow = Mathf.Max(_slow, ld._slow);
            // при обновлении можно заново нанести урон (если хочетс€) Ч здесь мы сбросим флаг,
            // чтобы урон повторно сработал при следующем тике:
            _damageApplied = false;
            Elapsed = 0f;
        }
    }

    // если у теб€ централизованна€ система модификаторов Ч отдаЄм текущую величину замедлени€
    public Dictionary<ModifierType, float> GetModifiers()
    {
        return new Dictionary<ModifierType, float>
        {
            { ModifierType.Slow, _slow }
        };
    }
}
