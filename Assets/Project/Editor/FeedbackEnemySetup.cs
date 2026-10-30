using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class FeedbackEnemySetup
{
    public static void Run()
    {
        string directory = "Assets/Project/Prefabs/Enemy/";
        var cactus = AssetDatabase.LoadAssetAtPath<GameObject>(directory + "Cactus.prefab");
        var template = new SerializedObject(cactus.GetComponentInChildren<EnemyStatusCanvas>(true));
        var iconPrefab = template.FindProperty("effectUIPrefab").objectReferenceValue;
        string[] names = { "golem_small", "fish", "golem_big", "Cyclops" };
        float[] health = { 160f, 190f, 280f, 400f };
        float[] speed = { 0.95f, 1.4f, 0.8f, 0.7f };
        for (int i = 0; i < names.Length; i++)
        {
            string path = directory + names[i] + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
                var enemy = root.GetComponent<StandardEnemy>();
                if (enemy == null) enemy = root.AddComponent<StandardEnemy>();
                var canvas = root.GetComponentInChildren<Canvas>(true);
                var status = canvas.GetComponent<EnemyStatusCanvas>();
                if (status == null) status = canvas.gameObject.AddComponent<EnemyStatusCanvas>();
                var fill = canvas.GetComponentsInChildren<Image>(true).First(image => image.type == Image.Type.Filled);
                var effects = canvas.transform.Find("Effects");
                if (effects == null)
                {
                    var rect = new GameObject("Effects", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
                    rect.SetParent(canvas.transform, false);
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.sizeDelta = new Vector2(120f, 30f);
                    rect.anchoredPosition = new Vector2(0f, -25f);
                    var layout = rect.GetComponent<HorizontalLayoutGroup>();
                    layout.spacing = 6f;
                    layout.childControlWidth = false;
                    layout.childControlHeight = false;
                    layout.childForceExpandWidth = false;
                    layout.childForceExpandHeight = false;
                    effects = rect;
                }
                var hit = root.transform.Find("HitTransform");
                if (hit == null)
                {
                    hit = new GameObject("HitTransform").transform;
                    hit.SetParent(root.transform, false);
                    hit.localPosition = root.GetComponent<CapsuleCollider>().center;
                }
                var serialized = new SerializedObject(enemy);
                serialized.FindProperty("baseSpeed").floatValue = speed[i];
                serialized.FindProperty("maxHealth").floatValue = health[i];
                serialized.FindProperty("killReward").floatValue = 150f + i * 25f;
                serialized.FindProperty("gearsKillReward").floatValue = 3f + i;
                serialized.FindProperty("contactDamage").floatValue = 20f;
                serialized.FindProperty("hitTransform").objectReferenceValue = hit;
                serialized.FindProperty("enemyStatusCanvas").objectReferenceValue = status;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                serialized = new SerializedObject(status);
                serialized.FindProperty("healthBar").objectReferenceValue = fill.gameObject;
                serialized.FindProperty("effectsContainer").objectReferenceValue = effects;
                serialized.FindProperty("effectUIPrefab").objectReferenceValue = iconPrefab;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("FEEDBACK_ENEMY_SETUP_SUCCEEDED");
    }
}
