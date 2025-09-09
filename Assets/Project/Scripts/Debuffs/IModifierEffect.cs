using System.Collections.Generic;

public interface IModifierEffect : IStatusEffect
{
    //ModifierType, value
    Dictionary<ModifierType, float> GetModifiers();
}