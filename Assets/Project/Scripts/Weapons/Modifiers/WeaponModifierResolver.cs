using System.Collections.Generic;

public class WeaponModifierResolver
{
    private readonly Dictionary<WeaponModifierType, IWeaponModifier> _weaponModifiers;

    public WeaponModifierResolver(
        EveryNthDamageBoostModifier everyNthDamageBoostModifier)
    {
        _weaponModifiers = new Dictionary<WeaponModifierType, IWeaponModifier>
        {
            {WeaponModifierType.ModifyEveryNShot, everyNthDamageBoostModifier },
        };
    }

    public List<IWeaponModifier> ResolveModifiers(IEnumerable<WeaponModifierType> types)
    {
        var list = new List<IWeaponModifier>();
        foreach (var t in types)
        {
            switch (t)
            {
                case WeaponModifierType.ModifyEveryNShot:
                    list.Add(new EveryNthDamageBoostModifier());
                    break;
                default:
                    throw new KeyNotFoundException($"Modifier not implemented: {t}");
            }
        }
        return list;
    }
}
