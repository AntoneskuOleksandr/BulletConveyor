using System.Collections.Generic;

public interface IReadOnlyPlayerData
{
    int Currency { get; }
    IReadOnlyList<WeaponEntry> Weapons { get; }
    IReadOnlyList<int> SelectedWeaponIds { get; }
}
