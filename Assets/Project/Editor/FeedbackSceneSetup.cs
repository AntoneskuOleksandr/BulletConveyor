using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class FeedbackSceneSetup
{
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(AndroidBuild.Scenes[0]);
        var display = Find<SelectedWeaponsDisplay>(scene);
        if (display == null) display = new GameObject("Selected weapon platforms").AddComponent<SelectedWeaponsDisplay>();
        Set(display, "platformPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project/Prefabs/Environment/Platform/platform.prefab"));
        SetLighting(scene);
        EditorSceneManager.SaveScene(scene);
        scene = EditorSceneManager.OpenScene(AndroidBuild.Scenes[1]);
        var speed = Find<GameSpeed>(scene);
        if (speed == null) speed = new GameObject("Game speed").AddComponent<GameSpeed>();
        Set(Find<GameplayInstaller>(scene), "gameSpeed", speed);
        SetLighting(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("FEEDBACK_SCENE_SETUP_SUCCEEDED");
    }

    private static T Find<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
        .SelectMany(root => root.GetComponentsInChildren<T>(true)).FirstOrDefault();

    private static void Set(Object target, string property, Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetLighting(Scene scene)
    {
        var sun = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>(true))
            .First(light => light.type == LightType.Directional);
        sun.enabled = true;
        sun.gameObject.SetActive(true);
        sun.lightmapBakeType = LightmapBakeType.Realtime;
        sun.intensity = 1.35f;
        sun.color = new Color(1f, 0.97f, 0.9f);
        sun.renderMode = LightRenderMode.ForcePixel;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.7f;
        sun.shadowBias = 0.03f;
        sun.shadowNormalBias = 0.25f;
        sun.shadowResolution = LightShadowResolution.Medium;
        RenderSettings.sun = sun;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.58f, 0.67f, 0.78f);
        RenderSettings.ambientEquatorColor = new Color(0.42f, 0.45f, 0.5f);
        RenderSettings.ambientGroundColor = new Color(0.24f, 0.22f, 0.2f);
        RenderSettings.ambientIntensity = 1f;
        EditorSceneManager.MarkSceneDirty(scene);
    }
}
