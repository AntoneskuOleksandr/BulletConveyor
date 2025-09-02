using System.Collections.Generic;

public interface IModifierEffect : IStatusEffect
{
    Dictionary<ModifierType, float> GetModifiers();
}