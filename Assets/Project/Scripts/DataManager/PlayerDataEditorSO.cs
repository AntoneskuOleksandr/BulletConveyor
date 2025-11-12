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

    [Range(0, 7)]
    public int randomWeaponCount = 0;

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

    [ContextMenu("Generate Random Weapon IDs")]
    private void GenerateRandomWeaponIDs()
    {
        selectedWeaponIds.Clear();

        List<int> allIds = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

        for (int i = 0; i < allIds.Count; i++)
        {
            int randomIndex = Random.Range(i, allIds.Count);
            (allIds[i], allIds[randomIndex]) = (allIds[randomIndex], allIds[i]);
        }

        for (int i = 0; i < randomWeaponCount && i < allIds.Count; i++)
        {
            selectedWeaponIds.Add(allIds[i]);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        Debug.Log($"[PlayerDataEditorSO] Generated {selectedWeaponIds.Count} Weapon IDs: {string.Join(", ", selectedWeaponIds)}");
    }
}
