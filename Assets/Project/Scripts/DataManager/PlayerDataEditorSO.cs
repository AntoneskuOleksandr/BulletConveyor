using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDataEditorSO", menuName = "Game/Player Data Editor", order = 0)]
public class PlayerDataEditorSO : ScriptableObject
{
    [Header("General")]
    public int playerLevel;
    public int selectedLevelIndex;
    public bool isTutorialCompleted;

    [Header("Currencies")]
    public List<CurrencyEntry> currencies = new();

    [Header("Weapons")]
    public List<WeaponEntry> weapons = new();

    [Header("Selected Weapon IDs")]
    public List<int> selectedWeaponIds = new();

    public PlayerData GetPlayerData()
    {
        var data = new PlayerData
        {
            playerLevel = playerLevel,
            selectedLevelIndex = selectedLevelIndex,
            isTutorialCompleted = isTutorialCompleted,
            currencies = new List<CurrencyEntry>(currencies),
            weapons = new List<WeaponEntry>(weapons),
            selectedWeaponIds = new List<int>(selectedWeaponIds)
        };

        return data;
    }
}
