using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ShapePrefabCreator : EditorWindow
{
    public GameObject cubePrefab;
    public List<string> shapeRows = new() { "11" };
    public float cubeSize = 1.75f;

    private const string BaseFolder = "Assets/Project/Prefabs/Blocks";

    [MenuItem("Tools/Create Shape Prefab")]
    public static void ShowWindow() => GetWindow<ShapePrefabCreator>("Shape Prefab Creator");

    private void OnEnable()
    {
        if (cubePrefab == null)
        {
            cubePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{BaseFolder}/Template/Box.prefab"
            );
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Настройки генерации фигуры", EditorStyles.boldLabel);

        cubePrefab = (GameObject)EditorGUILayout.ObjectField("Cube Prefab", cubePrefab, typeof(GameObject), false);
        cubeSize = EditorGUILayout.FloatField("Cube Size", cubeSize);

        EditorGUILayout.LabelField("Shape Rows (use '1' for cube, '0' for empty):");

        for (int i = 0; i < shapeRows.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            shapeRows[i] = EditorGUILayout.TextField(shapeRows[i]);
            if (GUILayout.Button("-", GUILayout.Width(20)))
                shapeRows.RemoveAt(i);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Row"))
            shapeRows.Add("");

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate and Save Prefab"))
        {
            if (!ValidateInput()) return;
            CreateShapePrefab();
        }
    }

    private bool ValidateInput()
    {
        if (cubePrefab == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Укажите Cube Prefab!", "ОК");
            return false;
        }
        if (shapeRows.Count == 0)
        {
            EditorUtility.DisplayDialog("Ошибка", "Shape Rows пусты!", "ОК");
            return false;
        }
        if (!ValidateShapeRows())
        {
            EditorUtility.DisplayDialog("Ошибка",
                "Все строки должны быть одинаковой длины и содержать только '0' или '1'!",
                "ОК");
            return false;
        }
        return true;
    }

    private bool ValidateShapeRows()
    {
        int len = shapeRows[0].Length;
        foreach (var row in shapeRows)
        {
            if (row.Length != len) return false;
            foreach (char c in row)
                if (c != '0' && c != '1') return false;
        }
        return true;
    }

    private (int minX, int maxX, int minY, int maxY) GetBounds()
    {
        int rows = shapeRows.Count;
        int cols = shapeRows[0].Length;
        int minX = cols, maxX = -1, minY = rows, maxY = -1;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (shapeRows[y][x] == '1')
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }
        return (minX, maxX, minY, maxY);
    }

    private void CreateShapePrefab()
    {
        var bounds = GetBounds();
        if (bounds.maxX == -1 || bounds.maxY == -1)
        {
            EditorUtility.DisplayDialog("Ошибка", "Фигура не содержит ни одного куба!", "ОК");
            return;
        }

        int shapeWidth = bounds.maxX - bounds.minX + 1;
        int shapeHeight = bounds.maxY - bounds.minY + 1;
        float offsetX = (shapeWidth - 1) * cubeSize / 2f;
        float offsetZ = (shapeHeight - 1) * cubeSize / 2f;

        GameObject shapeRoot = new GameObject("GeneratedShape");
        var blockParent = shapeRoot.AddComponent<BlockParent>();
        var shapeData = new List<RowData>();

        for (int y = bounds.maxY; y >= bounds.minY; y--)
        {
            string row = shapeRows[y];
            RowData rowData = new() { row = new int[row.Length] };

            for (int x = bounds.minX; x <= bounds.maxX; x++)
            {
                int value = row[x] == '1' ? 1 : 0;
                rowData.row[x] = value;

                if (value == 1)
                {
                    Vector3 localPos = new(
                        (x - bounds.minX) * cubeSize - offsetX,
                        0,
                        (bounds.maxY - y) * cubeSize - offsetZ
                    );
                    var cubeInstance = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab, shapeRoot.transform);
                    cubeInstance.transform.localPosition = localPos;
                }
            }
            shapeData.Add(rowData);
        }

        blockParent.shape = shapeData;
        blockParent.scaleX = shapeWidth;
        blockParent.scaleZ = shapeHeight;

        // Canvas
        GameObject canvasGO = new GameObject("ShapeCanvas");
        canvasGO.transform.SetParent(shapeRoot.transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(shapeWidth * cubeSize, shapeHeight * cubeSize);
        canvasRect.localPosition = new Vector3(0, cubeSize / 2f, 0);
        canvasRect.localRotation = Quaternion.Euler(90, 0, 0);

        // Контейнер с HorizontalLayoutGroup
        GameObject containerGO = new GameObject("CountContainer");
        containerGO.transform.SetParent(canvasGO.transform, false);

        var layout = containerGO.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;   // Растягиваем детей по ширине
        layout.childForceExpandHeight = true;  // Растягиваем по высоте

        RectTransform containerRect = containerGO.GetComponent<RectTransform>();
        containerRect.sizeDelta = canvasRect.sizeDelta;
        containerRect.localPosition = Vector3.zero;
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);

        // Текст - занимает ~70% ширины
        GameObject textGO = new GameObject("CountText");
        textGO.transform.SetParent(containerGO.transform, false);

        var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.alignment = TMPro.TextAlignmentOptions.Right; // Текст по правому краю контейнера (чтобы иконка была рядом справа)
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0;
        tmp.color = Color.white;

        int blockCount = CountBlocks(shapeData);
        tmp.text = "+" + blockCount.ToString();

        RectTransform textRect = tmp.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x * 0.5f, canvasRect.sizeDelta.y);
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(1, 0.5f); // Правый центр

        // Иконка пули - занимает ~30% ширины
        GameObject iconGO = new GameObject("BulletIcon");
        iconGO.transform.SetParent(containerGO.transform, false);

        var iconImage = iconGO.AddComponent<UnityEngine.UI.Image>();
        iconImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Project/UI/Bullet_Icon.png");
        iconImage.preserveAspect = true;

        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x * 0.5f, canvasRect.sizeDelta.y);
        iconRect.anchorMin = new Vector2(0, 0);
        iconRect.anchorMax = new Vector2(1, 1);
        iconRect.pivot = new Vector2(0, 0.5f); // Левый центр

        // Сохраняем и очищаем
        string targetFolder = GetTargetFolder(IsShapeRectangular(bounds));
        SavePrefab(shapeRoot, shapeWidth, shapeHeight, targetFolder);

        DestroyImmediate(shapeRoot);
    }

    private int CountBlocks(List<RowData> shape)
    {
        int count = 0;
        foreach (var row in shape)
            foreach (int cell in row.row)
                if (cell == 1) count++;
        return count;
    }

    private bool IsShapeRectangular((int minX, int maxX, int minY, int maxY) bounds)
    {
        for (int y = bounds.minY; y <= bounds.maxY; y++)
            for (int x = bounds.minX; x <= bounds.maxX; x++)
                if (shapeRows[y][x] != '1') return false;
        return true;
    }

    private string GetTargetFolder(bool isRectangle)
    {
        string target = isRectangle ? $"{BaseFolder}/Nx{shapeRows.Count}" : $"{BaseFolder}/Specific";
        if (!AssetDatabase.IsValidFolder(target))
            AssetDatabase.CreateFolder(BaseFolder, isRectangle ? $"Nx{shapeRows.Count}" : "Specific");
        return target;
    }

    private void SavePrefab(GameObject shapeRoot, int width, int height, string folder)
    {
        string shapeHash = GetShapeHash(shapeRows);
        string prefabName = $"{width}x{height}_{shapeHash}.prefab";
        string prefabPath = $"{folder}/{prefabName}";

        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            Debug.LogWarning($"Префаб {prefabName} уже существует, перезаписываю.");
            AssetDatabase.DeleteAsset(prefabPath);
        }

        PrefabUtility.SaveAsPrefabAsset(shapeRoot, prefabPath);
        AssetDatabase.SaveAssets();

        if (AssetDatabase.LoadAssetAtPath<Object>(folder) is Object folderAsset)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = folderAsset;
        }

        Debug.Log($"Префаб создан: <a href=\"{folder}\">{folder}</a>");
    }

    private string GetShapeHash(List<string> shape)
    {
        string shapeString = string.Concat(shape);
        using MD5 md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(shapeString));

        StringBuilder sb = new();
        foreach (byte b in hashBytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString()[..6];
    }
}
