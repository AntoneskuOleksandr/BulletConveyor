using System.Collections.Generic;

public interface IReadOnlyPlayerData
{
    IReadOnlyList<CurrencyEntry> Currencies { get; }
    IReadOnlyList<int> SelectedWeaponIds { get; }
}
