using System.Collections.Generic;

public class WeaponModifierResolver
{
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
                case WeaponModifierType.SpreadShot:
                    list.Add(new SpreadShotModifier());
                    break;
                default:
                    throw new KeyNotFoundException($"Modifier not implemented: {t}");
            }
        }
        return list;
    }
}
