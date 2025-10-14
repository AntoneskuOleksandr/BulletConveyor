using System.Collections.Generic;

public readonly struct WeaponProgressRO
{
    public int WeaponId { get; }
    public int DamageLevel { get; }
    public int FireRateLevel { get; }
    public int HealthLevel { get; }

    public WeaponProgressRO(int id, int dmg, int fr, int hp)
    {
        WeaponId = id;
        DamageLevel = dmg;
        FireRateLevel = fr;
        HealthLevel = hp;
    }
}

public interface IReadOnlyPlayerData
{
    int Currency { get; }
    WeaponProgressRO GetProgress(int weaponId);
    IReadOnlyList<WeaponProgressRO> GetAllProgress();
    IReadOnlyList<int> SelectedWeaponIds { get; }
}

// Read-only фасад для UI
public class PlayerStateView : IReadOnlyPlayerData
{
    private readonly PlayerData _data;

    public PlayerStateView(PlayerData data) => _data = data;

    public int Currency => _data.currency;

    public WeaponProgressRO GetProgress(int id)
    {
        var e = _data.weapons.Find(w => w.id == id);
        return e.id == 0 && id != 0
            ? new WeaponProgressRO(id, 0, 0, 0)
            : new WeaponProgressRO(e.id, e.dmgLvl, e.fireRateLvl, e.healthLvl);
    }

    public IReadOnlyList<WeaponProgressRO> GetAllProgress()
    {
        var list = new List<WeaponProgressRO>(_data.weapons.Count);
        foreach (var w in _data.weapons)
            list.Add(new WeaponProgressRO(w.id, w.dmgLvl, w.fireRateLvl, w.healthLvl));
        return list;
    }

    public IReadOnlyList<int> SelectedWeaponIds => _data.selectedWeaponIds.AsReadOnly();
}
