

public interface IStatusEffect
{
    OnHitEffectType EffectType { get; }
    EffectStackType StackType { get; }

    float Duration { get; }

    float Elapsed { get; }

    float DurationLeft { get; }

    bool IsFinished { get; }

    void Refresh(IStatusEffect newEffect);
}

public enum EffectStackType
{
    Refreshable,
    Stackable
}
