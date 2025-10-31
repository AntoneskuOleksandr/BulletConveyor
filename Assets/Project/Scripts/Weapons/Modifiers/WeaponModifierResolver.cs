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
            if (_weaponModifiers.TryGetValue(t, out var s))
                list.Add(s);
            else
                throw new KeyNotFoundException($"IWeaponModifier not registered: {t}");
        }
        return list;
    }
}
