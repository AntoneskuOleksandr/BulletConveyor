using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class MenuLifecycleValidation
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int checks;
    private static int exitCode;
    private static bool previousOptionsEnabled;
    private static EnterPlayModeOptions previousOptions;

    public static void Run()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Run menu lifecycle validation in a separate batch-mode checkout.");

        checks = 0;
        exitCode = 0;
        previousOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        previousOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            try
            {
                CheckOrder(true);
                CheckOrder(false);
                Debug.Log($"M1_MENU_CHECKS_SUCCEEDED: {checks} checks passed");
            }
            catch (Exception exception)
            {
                exitCode = 1;
                Debug.LogException(exception);
            }
            finally
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                EditorSettings.enterPlayModeOptionsEnabled = previousOptionsEnabled;
                EditorSettings.enterPlayModeOptions = previousOptions;
                EditorApplication.Exit(exitCode);
            }
        }
    }

    private static void CheckOrder(bool menuFirst)
    {
        using var fixture = new Fixture();
        if (menuFirst)
        {
            fixture.EnableMenu();
            Check(!fixture.Play.interactable, "Menu-first initialization waits for a selected level");
            fixture.EnableDisplay();
        }
        else
        {
            fixture.EnableDisplay();
            fixture.EnableMenu();
        }

        Check(fixture.Play.interactable, $"Play becomes available with menuFirst={menuFirst}");
        Check(fixture.Display.TryGetCurrentLevel(out int level) && level == 3,
            "Saved level IDs are resolved independently of their list positions");
        Transform preview = fixture.Previews.GetChild(0);
        for (int i = 0; i < 3; i++)
        {
            fixture.DisableMenu();
            fixture.DisableDisplay();
            fixture.EnableMenu();
            fixture.EnableDisplay();
        }
        Check(fixture.Play.interactable && fixture.Previews.childCount == 1 && fixture.Previews.GetChild(0) == preview,
            "Reopening the menu preserves one preview and a usable Play button");

        fixture.Next.onClick.Invoke();
        Check(fixture.Display.GetCurrentLevel() == 7 && fixture.ExternalClicks == 1,
            "One next click changes one level and retains other button listeners");
        fixture.Next.onClick.Invoke();
        Check(fixture.Display.GetCurrentLevel() == 0, "Next wraps to the first level");
        fixture.Previous.onClick.Invoke();
        Check(fixture.Display.GetCurrentLevel() == 7, "Previous wraps to the last level");
        fixture.Data.playerLevel = 0;
        fixture.Previous.onClick.Invoke();
        Check(!fixture.Play.interactable, "Locked levels cannot start after a menu refresh");
    }

    private static void Check(bool success, string name)
    {
        if (!success) throw new InvalidOperationException("M1_MENU_CHECK_FAILED: " + name);
        checks++;
        Debug.Log("M1 PASS: " + name);
    }

    private static void Set(object target, string name, object value) =>
        target.GetType().GetField(name, PrivateInstance).SetValue(target, value);

    private static void Call(object target, string name) =>
        target.GetType().GetMethod(name, PrivateInstance).Invoke(target, null);

    private sealed class Fixture : IDisposable
    {
        public readonly GameObject Root;
        public readonly MainMenuUIManager Menu;
        public readonly LevelDisplay Display;
        public readonly PlayerData Data;
        public readonly Button Play;
        public readonly Button Next;
        public readonly Button Previous;
        public readonly Transform Previews;
        public int ExternalClicks;

        private readonly List<LevelSO> levels = new();
        private readonly string directory;
        private readonly BackgroundGenerator background;
        private bool menuEnabled;
        private bool displayEnabled;

        public Fixture()
        {
            Root = new GameObject("Menu lifecycle fixture");
            Root.SetActive(false);
            directory = Path.GetFullPath(Path.Combine("Temp/MenuLifecycle", Guid.NewGuid().ToString("N")));
            var repository = new DataRepository(Path.Combine(directory, "playerdata.json"));
            var manager = new DataManager(repository);
            Data = manager.LoadOrCreate();
            Data.playerLevel = 7;
            Data.selectedLevelIndex = 3;
            var economy = new EconomyService(Data, manager);
            var progress = new PlayerProgressService(Data, manager);
            var database = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(
                AssetDatabase.GUIDToAssetPath("333379f1220eaa947be4a0ba4ba6941e"));
            var weapons = new WeaponsService(Data, manager, database, economy, progress);
            var tutorial = new TutorialProgressService(Data, manager);
            Menu = Root.AddComponent<MainMenuUIManager>();
            Display = Root.AddComponent<LevelDisplay>();
            Play = Create<Button>("Play");
            Next = Create<Button>("Next");
            Previous = Create<Button>("Previous");
            Previews = Create<RectTransform>("Previews");
            background = Create<BackgroundGenerator>("Background");
            var template = Create<Transform>("Preview template").gameObject;
            foreach (int id in new[] { 0, 3, 7 })
            {
                var level = ScriptableObject.CreateInstance<LevelSO>();
                level.LevelIndex = id;
                level.MiniLevelGO = template;
                levels.Add(level);
            }
            Set(Menu, "startGameButton", Play);
            Set(Menu, "moneyText", Create<TextMeshProUGUI>("Money"));
            Set(Menu, "lockPanel", Create<RectTransform>("Lock").gameObject);
            Set(Menu, "_economyService", economy);
            Set(Menu, "_weaponsService", weapons);
            Set(Menu, "_playerProgressService", progress);
            Set(Menu, "_tutorialProgressService", tutorial);
            Set(Menu, "_levelDisplay", Display);
            Set(Display, "levels", levels);
            Set(Display, "miniLevels", Previews);
            Set(Display, "nextLevelButton", Next);
            Set(Display, "previousLevelButton", Previous);
            Set(Display, "backgroundGenerator", background);
            Set(Display, "_playerProgressService", progress);
            Set(Display, "_mainMenuUIManager", Menu);
            Next.onClick.AddListener(() => ExternalClicks++);
        }

        private T Create<T>(string name) where T : Component
        {
            var child = new GameObject(name);
            child.transform.SetParent(Root.transform, false);
            if (typeof(T) == typeof(Transform)) return child.transform as T;
            return child.AddComponent<T>();
        }

        public void EnableMenu() { menuEnabled = true; Call(Menu, "OnEnable"); }
        public void EnableDisplay() { displayEnabled = true; Call(Display, "OnEnable"); }
        public void DisableMenu() { Call(Menu, "OnDisable"); menuEnabled = false; }
        public void DisableDisplay() { Call(Display, "OnDisable"); displayEnabled = false; }

        public void Dispose()
        {
            if (menuEnabled) DisableMenu();
            if (displayEnabled) DisableDisplay();
            var texture = (Texture2D)typeof(BackgroundGenerator).GetField("_backgroundTexture", PrivateInstance).GetValue(background);
            if (texture != null) Object.DestroyImmediate(texture);
            Object.DestroyImmediate(Root);
            foreach (var level in levels) Object.DestroyImmediate(level);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}
