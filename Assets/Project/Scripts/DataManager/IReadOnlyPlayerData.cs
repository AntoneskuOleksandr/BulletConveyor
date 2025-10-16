using System.Collections.Generic;

public interface IReadOnlyPlayerData
{
    IReadOnlyList<CurrencyEntry> Currencies { get; }
    IReadOnlyList<WeaponEntry> Weapons { get; }
    IReadOnlyList<int> SelectedWeaponIds { get; }
}
