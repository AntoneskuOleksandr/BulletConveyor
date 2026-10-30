using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class BuildValidation
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int passed;

    [MenuItem("Build/Bullet Conveyor/Validate")]
    public static void RunAll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before validating or building.");
        if (!Application.isBatchMode)
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save open scenes before validating or building.");

        passed = 0;
        string directory = Path.Combine(Path.GetTempPath(), "BulletConveyorChecks", Guid.NewGuid().ToString("N"));
        try
        {
            var repository = new DataRepository(Path.Combine(directory, "playerdata.json"));
            var manager = new DataManager(repository);
            var data = manager.LoadOrCreate();
            Check(data.currencies.Count == Enum.GetValues(typeof(CurrencyType)).Length &&
                data.PlayerLevel == 0 && !data.IsTutorialCompleted && data.SelectedWeaponIds.Count == 0,
                "Fresh profile starts without debug progress and has every currency");

            var economy = new EconomyService(data, manager);
            economy.AddCurrency(CurrencyType.Gears, 100);
            Check(economy.TrySpend(CurrencyType.Gears, 25) &&
                repository.Load().currencies.Find(c => c.type == CurrencyType.Gears).amount == 75,
                "Currency gains and spending survive reload");

            var progress = new PlayerProgressService(data, manager);
            var database = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(
                AssetDatabase.GUIDToAssetPath("333379f1220eaa947be4a0ba4ba6941e"));
            var weapons = new WeaponsService(data, manager, database, economy, progress);
            var tutorial = new TutorialProgressService(data, manager);
            CheckMenu(data, economy, progress, weapons, tutorial);

            Check(weapons.SelectWeapon(0), "Starter weapon can be equipped at level zero");
            tutorial.CompleteTutorial();
            var reloaded = new DataManager(repository).LoadOrCreate();
            Check(reloaded.IsTutorialCompleted && reloaded.SelectedWeaponIds.Contains(0),
                "Tutorial completion and starter loadout survive reload");

            var fixture = new PlayerData { playerLevel = 3 };
            var fixtureRepository = new DataRepository(Path.Combine(directory, "debug-playerdata.json"));
            var fixtureManager = new DataManager(fixtureRepository);
            fixtureManager.LoadOrCreate(fixture);
            fixture.playerLevel = 4;
            fixtureManager.Save();
            Check(fixtureRepository.Load().PlayerLevel == 4 && repository.Load().PlayerLevel == 0,
                "Editor fixture mutations save the bound instance and preserve normal progress");

            File.WriteAllText(Path.Combine(directory, "legacy.json"),
                "{\"currencies\":null,\"weapons\":null,\"selectedWeaponIds\":null}");
            var legacy = new DataManager(new DataRepository(Path.Combine(directory, "legacy.json"))).LoadOrCreate();
            Check(legacy.currencies.Count > 0 && legacy.weapons != null && legacy.selectedWeaponIds != null,
                "Legacy profiles with null collections are normalized");

            CheckEmptyDragHit();
            CheckScenes();
            Check(database.weapons.All(weapon => weapon.UpgradeLevels.Length == 5 &&
                weapon.UpgradeLevels.All(level => level.Cost > 0 && level.DamageBonus > 0 && level.HealthBonus > 0 && level.FireRateBonus > 0f)),
                "Every weapon has five complete upgrade tiers");
            var audio = Resources.Load<GameAudioConfig>("GameAudio");
            Check(audio != null && audio.menuMusic != null && audio.combatMusic != null && audio.click != null && audio.shot != null,
                "Music and feedback assets are included in Resources");
            var levels = AssetDatabase.FindAssets("t:LevelSO", new[] { "Assets/Project/ScriptableObjects/Levels" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<LevelSO>(AssetDatabase.GUIDToAssetPath(guid)));
            Check(levels.Count() == 5 && levels.All(level => level.levelConfig != null &&
                level.levelConfig.environmentPrefab != null && level.levelConfig.enemyType1 != null && level.levelConfig.enemyType2 != null),
                "All five selectable maps have complete gameplay configuration");
            string damagedPath = Path.Combine(directory, "damaged.json");
            var recoverable = new DataRepository(damagedPath);
            recoverable.Save(new PlayerData { playerLevel = 2 });
            recoverable.Save(new PlayerData { playerLevel = 3 });
            File.WriteAllText(damagedPath, "{broken json");
            var recovered = new DataManager(recoverable).LoadOrCreate();
            Check(recovered.PlayerLevel == 2 && Directory.GetFiles(directory, "damaged.json.corrupt-*").Length == 1 &&
                recoverable.Load().PlayerLevel == 2, "A damaged save preserves the damaged file and restores a readable backup");
            Debug.Log($"BUILD_VALIDATION_SUCCEEDED: {passed} checks passed");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private static void CheckMenu(PlayerData data, EconomyService economy, PlayerProgressService progress,
        WeaponsService weapons, TutorialProgressService tutorial)
    {
        var host = new GameObject("Menu validation");
        host.SetActive(false);
        try
        {
            var menu = host.AddComponent<MainMenuUIManager>();
            var display = host.AddComponent<LevelDisplay>();
            var button = host.AddComponent<Button>();
            Set(menu, "startGameButton", button);
            Set(menu, "_economyService", economy);
            Set(menu, "_weaponsService", weapons);
            Set(menu, "_playerProgressService", progress);
            Set(menu, "_tutorialProgressService", tutorial);
            Set(menu, "_levelDisplay", display);
            Set(display, "_playerProgressService", progress);
            typeof(MainMenuUIManager).GetMethod("UpdateStartButtonState", PrivateInstance).Invoke(menu, null);
            Check(!button.interactable, "Play waits until the level display is initialized");
            Set(display, "currentLevel", AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/Project/ScriptableObjects/Levels/Desert.asset"));
            typeof(MainMenuUIManager).GetMethod("UpdateStartButtonState", PrivateInstance).Invoke(menu, null);
            Check(button.interactable, "Fresh profile can start the tutorial without selecting a weapon");
            data.isTutorialCompleted = true;
            typeof(MainMenuUIManager).GetMethod("UpdateStartButtonState", PrivateInstance).Invoke(menu, null);
            Check(!button.interactable, "Completed tutorial still requires a loadout for normal play");
            data.isTutorialCompleted = false;
        }
        finally { UnityEngine.Object.DestroyImmediate(host); }
    }

    private static void CheckEmptyDragHit()
    {
        var host = new GameObject("Drag validation");
        host.SetActive(false);
        try
        {
            var block = host.AddComponent<BlockParent>();
            var controller = new BlockDragController(null, null, null, null);
            Set(controller, "session", new DragSession(block, 0));
            object[] args = { default(RaycastHit), null, null };
            bool merged = (bool)typeof(BlockDragController).GetMethod("TryMerge", PrivateInstance).Invoke(controller, args);
            Check(!merged, "Releasing over empty space does not dereference a missing collider");
        }
        finally { UnityEngine.Object.DestroyImmediate(host); }
    }

    private static void CheckScenes()
    {
        var setup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            foreach (string path in AndroidBuild.Scenes)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int missing = 0;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                        missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                Check(missing == 0, $"{path} has no missing scripts");
                if (path.EndsWith("GameScene.unity"))
                {
                    var tutorial = scene.GetRootGameObjects()
                        .SelectMany(root => root.GetComponentsInChildren<TutorialManager>(true)).Single();
                    var serialized = new SerializedObject(tutorial);
                    var pointer = (GameObject)serialized.FindProperty("handPointer").objectReferenceValue;
                    var instruction = (Graphic)serialized.FindProperty("instructionText").objectReferenceValue;
                    Check(pointer.GetComponentsInChildren<Graphic>(true).All(graphic => !graphic.raycastTarget) &&
                        !instruction.raycastTarget && instruction.GetComponentInParent<Canvas>().renderMode != RenderMode.WorldSpace,
                        "Tutorial overlays pass taps through and instructions use the screen canvas");
                }
            }
        }
        finally
        {
            if (setup.Any(scene => scene.isActive)) EditorSceneManager.RestoreSceneManagerSetup(setup);
            else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
    }

    private static void Set(object target, string name, object value) =>
        target.GetType().GetField(name, PrivateInstance).SetValue(target, value);

    private static void Check(bool success, string name)
    {
        if (!success) throw new InvalidOperationException("BUILD_VALIDATION_FAILED: " + name);
        passed++;
        Debug.Log("PASS: " + name);
    }
}
