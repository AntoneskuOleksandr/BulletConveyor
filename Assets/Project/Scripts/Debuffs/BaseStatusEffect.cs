using UnityEngine;

public abstract class BaseStatusEffect : IStatusEffect
{
    public abstract OnHitEffectType EffectType { get; }
    public abstract EffectStackType StackType { get; }

    public float Duration { get; protected set; }
    public float Elapsed { get; protected set; }
    public float DurationLeft => Mathf.Max(Duration - Elapsed, 0f);
    public virtual bool IsFinished => Elapsed >= Duration;

    public virtual void Refresh(IStatusEffect newEffect)
    {
        if (StackType == EffectStackType.Refreshable)
        {
            OnRefresh(newEffect);
        }
    }

    protected abstract void OnRefresh(IStatusEffect newEffect);
}
