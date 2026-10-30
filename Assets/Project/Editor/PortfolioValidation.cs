using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;
using static UnityEngine.Object;

public class PortfolioValidation
{
    private static int checks;
    private static string directory;
    private static bool previousOptions;
    private static EnterPlayModeOptions previousFlags;
    private static readonly BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    private bool failed;

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a separate batch-mode checkout.");
        BuildValidation.RunAll();
        directory = Path.Combine(Path.GetTempPath(), "BulletPortfolio-" + Guid.NewGuid().ToString("N"));
        DataRepository.ValidationPath = Path.Combine(directory, "playerdata.json");
        previousOptions = EditorSettings.enterPlayModeOptionsEnabled;
        previousFlags = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene(AndroidBuild.Scenes[0]);
        EditorApplication.playModeStateChanged += OnPlay;
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= OnPlay;
        var host = new GameObject("Portfolio validation");
        DontDestroyOnLoad(host);
        new PortfolioValidation().Start(host.AddComponent<ValidationCoroutineRunner>());
    }

    private void Start(ValidationCoroutineRunner runner)
    {
        Application.logMessageReceived += OnLog;
        runner.StartCoroutine(Guard(Validate()));
    }

    private void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Assert ||
            (type == LogType.Warning && message.Contains("DOTWEEN") && message.Contains("Target or field")))
        {
            failed = true;
            Debug.Log("PORTFOLIO_RUNTIME_ERROR: " + message);
        }
    }

    private IEnumerator Guard(IEnumerator routine)
    {
        while (true)
        {
            object next;
            try
            {
                if (!routine.MoveNext()) break;
                next = routine.Current;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                failed = true;
                break;
            }
            yield return next;
        }
        Application.logMessageReceived -= OnLog;
        DataRepository.ValidationPath = null;
        EditorSettings.enterPlayModeOptionsEnabled = previousOptions;
        EditorSettings.enterPlayModeOptions = previousFlags;
        Time.timeScale = 1f;
        if (!failed) Debug.Log($"PORTFOLIO_CHECKS_SUCCEEDED: {checks} checks passed");
        else Debug.LogError($"PORTFOLIO_CHECKS_FAILED after {checks} checks");
        EditorApplication.Exit(failed ? 1 : 0);
    }

    private IEnumerator Validate()
    {
        yield return null;
        var container = ProjectContext.Instance.Container;
        var data = container.Resolve<IReadOnlyPlayerData>();
        var weapons = container.Resolve<WeaponsService>();
        var economy = container.Resolve<EconomyService>();
        var progress = container.Resolve<PlayerProgressService>();
        var tutorialProgress = container.Resolve<TutorialProgressService>();
        var database = container.Resolve<WeaponDatabaseSO>();
        foreach (var weapon in database.weapons)
        {
            Check(weapon.UpgradeLevels.Length == 5 && weapons.GetProgressRO(weapon.WeaponId).DamageLevel == 0,
                weapon.name + " has purchasable tiers and starts at zero");
        }
        var starter = database.weapons.Find(w => w.WeaponId == 0);
        economy.AddCurrency(CurrencyType.Gears, 25f);
        Check(weapons.TryUpgrade(starter, UpgradeType.Damage) && economy.GetCurrency(CurrencyType.Gears) == 0f,
            "A purchase charges exactly one tier");
        Check(!weapons.TryUpgrade(starter, UpgradeType.Damage) && weapons.GetProgressRO(0).DamageLevel == 1,
            "Unaffordable upgrade preserves progress");
        Check(new DataRepository(DataRepository.ValidationPath).Load().weapons.Find(w => w.id == 0).dmgLvl == 1,
            "Upgrade persists to disk");
        Check(FindAnyObjectByType<GameAudio>() != null && Resources.Load<GameAudioConfig>("GameAudio").shot != null,
            "Audio service and shot asset are present");

        var menu = FindAnyObjectByType<MainMenuController>();
        Get<Button>(menu, "weaponsButton").onClick.Invoke();
        yield return null;
        var upgrades = FindObjectsByType<UpgradeWeaponPanel>(FindObjectsInactive.Include).Single();
        upgrades.OnUpgradeButtonClick();
        Check(upgrades.gameObject.activeInHierarchy, "Upgrade controls open with one click");
        Get<Button>(menu, "homeButton").onClick.Invoke();
        Get<Button>(menu, "weaponsButton").onClick.Invoke();
        yield return null;
        Check(!upgrades.gameObject.activeSelf, "Leaving weapons closes the upgrade controls");
        Check(FindAnyObjectByType<WeaponsCarousel>().CurrentWeapon != null, "Rapid tab changes retain a weapon preview");
        upgrades.OnUpgradeButtonClick();
        Check(upgrades.gameObject.activeInHierarchy, "Reopened upgrade controls respond to the first click");
        Get<Button>(menu, "homeButton").onClick.Invoke();

        SceneManager.LoadScene("GameScene");
        yield return null;
        yield return null;
        var tutorial = FindAnyObjectByType<TutorialManager>();
        var blockManager = FindAnyObjectByType<BlockManager>();
        var board = FindAnyObjectByType<BoardManager>();
        var tiles = FindAnyObjectByType<TileManager>();
        var platforms = FindAnyObjectByType<MergePlatformsManager>();
        var wave = FindAnyObjectByType<WaveManager>();
        var buy = Get<GameObject>(tutorial, "buyButton").GetComponent<Button>();
        Check(tutorial.IsRunning && tutorial.CurrentStep == 0 && !wave.HasStarted, "Training waits for preparation");
        buy.onClick.Invoke();
        yield return null;
        Check(tutorial.CurrentStep == 1 && !buy.interactable, "Only a successful purchase advances the tutorial");
        var first = blockManager.LastCreatedBlock;
        var controller = new BlockDragController(tiles, board, platforms, Camera.main);
        Place(controller, tutorial, first, blockManager);
        Check(tutorial.CurrentStep == 2 && board.Blocks.Count == 1, "Board placement advances and provides the next purchase");
        buy.onClick.Invoke();
        yield return null;
        var second = blockManager.LastCreatedBlock;
        Check(tutorial.CurrentStep == 3 && second != first, "Second purchase provides the merge target");
        controller.StartDragging(first);
        Physics.SyncTransforms();
        Check(Physics.Raycast(second.transform.position + Vector3.up * 10f, Vector3.down, out var hit, 20f,
            LayerMask.GetMask("Block")), "Merge target can be hit");
        var result = controller.FinishDragging(hit);
        Check(result.Action == DragActionType.Merge, "Dragging the expected pair merges");
        controller.ExecuteAction(result, blockManager);
        yield return null;
        var merged = blockManager.LastCreatedBlock;
        Check(tutorial.CurrentStep == 4 && merged.rank == 1, "Merged block enters the final placement step");
        Place(controller, tutorial, merged, blockManager);
        Check(tutorial.CurrentStep == 5 && wave.HasStarted && board.Blocks.Count == 1, "Combat starts after the upgraded block is placed");
        var pause = FindObjectsByType<PauseMenuPanel>(FindObjectsInactive.Include).Single();
        pause.ShowPanel();
        yield return null;
        Check(Time.timeScale == 0f && !Get<RectTransform>(tutorial, "safeArea").gameObject.activeSelf,
            "Pause hides guidance and stops gameplay");
        typeof(PauseMenuPanel).GetMethod("HidePanel", Fields).Invoke(pause, null);
        Time.timeScale = 4f;
        float deadline = Time.realtimeSinceStartup + 75f;
        var enemies = FindAnyObjectByType<EnemyManager>();
        while (enemies.Enemies.Count == 0 && Time.realtimeSinceStartup < deadline) yield return null;
        Check(enemies.Enemies.Count > 0, "Training can spawn reachable enemies");
        Check(enemies.Enemies.All(e => e.GetComponent<Rigidbody>().isKinematic && !e.GetComponent<Animator>().applyRootMotion),
            "Agents have one movement owner");
        var win = FindObjectsByType<WinPanel>(FindObjectsInactive.Include).Single();
        while (!win.gameObject.activeSelf && Time.realtimeSinceStartup < deadline) yield return null;
        Check(win.gameObject.activeSelf, "Real turret combat wins both training waves");
        Get<Button>(win, "getButton").onClick.Invoke();
        yield return null;
        yield return null;
        Check(SceneManager.GetActiveScene().name == "MainMenu" && tutorialProgress.IsTutorialCompleted && progress.GetCurrentLevel() == 1,
            "Claiming victory saves training completion and returns to the menu");

        var levelAssets = AssetDatabase.FindAssets("t:LevelSO", new[] { "Assets/Project/ScriptableObjects/Levels" })
            .Select(g => AssetDatabase.LoadAssetAtPath<LevelSO>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(l => l.LevelIndex).ToArray();
        progress.SetPlayerLevel(8);
        foreach (var level in levelAssets)
        {
            progress.SetSelectedLevel(level.LevelIndex);
            SceneManager.LoadScene("GameScene");
            yield return null;
            yield return null;
            wave = FindAnyObjectByType<WaveManager>();
            wave.Freeze();
            var spawner = FindAnyObjectByType<EnemySpawner>();
            enemies = FindAnyObjectByType<EnemyManager>();
            Check(level.levelConfig != null && spawner.TryFindSpawnPosition(level.levelConfig.enemyType1, out _),
                level.name + " has a reachable, clear spawn area");
            Check(spawner.TryFindSpawnPosition(level.levelConfig.enemyType2, out _), level.name + " supports both enemy sizes");
            int spawned = 0;
            var spawning = spawner.SpawnWave(8, 1, 5, enemy => spawned++);
            float spawnDeadline = Time.realtimeSinceStartup + 20f;
            while (spawned < 8 && Time.realtimeSinceStartup < spawnDeadline)
            {
                if (!spawning.MoveNext()) break;
                yield return spawning.Current;
            }
            Check(spawned == 8, level.name + " spawns eight enemies without dropping requests");
            for (int i = 0; i < enemies.Enemies.Count; i++)
                for (int j = i + 1; j < enemies.Enemies.Count; j++)
                {
                    var a = enemies.Enemies[i];
                    var b = enemies.Enemies[j];
                    Check(Vector3.Distance(a.transform.position, b.transform.position) > 0.5f * (a.BodyRadius + b.BodyRadius),
                        level.name + " moving agents keep separation");
                }
        }
        var targetWeapon = FindAnyObjectByType<Weapon>();
        foreach (var weapon in FindObjectsByType<Weapon>()) weapon.TakeDamage(100000f);
        yield return null;
        yield return null;
        Check(FindObjectsByType<LosePanel>(FindObjectsInactive.Include).Single().gameObject.activeSelf,
            "Losing the last turret produces one defeat panel");
        Get<Button>(FindObjectsByType<LosePanel>(FindObjectsInactive.Include).Single(), "getButton").onClick.Invoke();
        yield return null;
        yield return null;
        Check(SceneManager.GetActiveScene().name == "MainMenu", "Defeat rewards return to a usable menu");
        int previousLevel = progress.GetCurrentLevel();
        tutorialProgress.RequestReplay();
        SceneManager.LoadScene("GameScene");
        yield return null;
        yield return null;
        tutorial = FindAnyObjectByType<TutorialManager>();
        Check(tutorial.IsRunning && tutorial.CurrentStep == 0, "Completed players can replay training");
        tutorial.SkipTutorial();
        Check(!tutorial.IsRunning && FindAnyObjectByType<BoardManager>().Blocks.Count == 1 &&
            FindAnyObjectByType<WaveManager>().HasStarted, "Skipping creates a playable ammo supply and starts combat");
        win = FindObjectsByType<WinPanel>(FindObjectsInactive.Include).Single();
        win.ShowPanel();
        Get<Button>(win, "getButton").onClick.Invoke();
        yield return null;
        Check(progress.GetCurrentLevel() == previousLevel, "Replay rewards do not unlock unrelated maps");
        Check(!failed, "No runtime exceptions occurred during the suite");
    }

    private static void Place(BlockDragController controller, TutorialManager tutorial, BlockParent block, BlockManager manager)
    {
        controller.StartDragging(block);
        Check(tutorial.TryGetBoardPlacement(block, out var position, out _), "A valid board destination is available");
        block.transform.position = position;
        block.SetPosition(position);
        var result = controller.FinishDragging(default);
        Check(result.Action == DragActionType.PlaceOnBoard, "A valid drop is accepted on the board");
        controller.ExecuteAction(result, manager);
    }

    private static T Get<T>(object target, string name)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(name, Fields);
            if (field != null) return (T)field.GetValue(target);
            type = type.BaseType;
        }
        throw new MissingFieldException(name);
    }

    private static void Check(bool success, string message)
    {
        if (!success) throw new InvalidOperationException("PORTFOLIO_CHECK_FAILED: " + message);
        checks++;
        Debug.Log("PASS: " + message);
    }
}
