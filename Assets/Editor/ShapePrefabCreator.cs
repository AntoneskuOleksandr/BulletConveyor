using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShapePrefabCreator : EditorWindow
{
    public GameObject cubePrefab;
    public List<string> shapeRows = new() { "11" };
    public float cubeSize = 1.75f;

    private const string BASEFOLDER = "Assets/Project/Prefabs/Blocks";

    [MenuItem("Tools/Create Shape Prefab")]
    public static void ShowWindow() => GetWindow<ShapePrefabCreator>("Shape Prefab Creator");

    private void OnEnable()
    {
        if (cubePrefab == null)
        {
            cubePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{BASEFOLDER}/Template/Box.prefab"
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
            GeneratePrefab(shapeRows, cubePrefab);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Regenerate All Prefabs"))
        {
            if (cubePrefab == null)
            {
                EditorUtility.DisplayDialog("Ошибка", "Укажите Cube Prefab!", "ОК");
            }
            else
            {
                RegenerateAllPrefabs();
            }
        }
    }

    #region Validation
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
    #endregion

    #region Prefab Generation
    public static void GeneratePrefab(List<string> shapeRows, GameObject cubePrefab)
    {
        if (shapeRows.Count == 0) return;
        var bounds = GetBoundsFromShapeRows(shapeRows);

        if (bounds.maxX == -1 || bounds.maxY == -1)
        {
            EditorUtility.DisplayDialog("Ошибка", "Фигура не содержит ни одного куба!", "ОК");
            return;
        }

        int shapeWidth = bounds.maxX - bounds.minX + 1;
        int shapeHeight = bounds.maxY - bounds.minY + 1;
        float cubeSize = 1.75f;
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
                        cubeSize / 2f,
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

        // Canvas, Text и Icon
        SetupCanvas(shapeRoot, blockParent, shapeWidth, shapeHeight);

        // Сохраняем префаб
        string targetFolder = GetTargetFolder(shapeRows);
        SavePrefabAsset(shapeRoot, shapeRows, shapeWidth, shapeHeight, targetFolder);

        Object.DestroyImmediate(shapeRoot);
    }

    private static void SetupCanvas(GameObject shapeRoot, BlockParent blockParent, int shapeWidth, int shapeHeight)
    {
        GameObject canvasGO = new GameObject("ShapeCanvas");
        canvasGO.transform.SetParent(shapeRoot.transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(shapeWidth * 1.75f, shapeHeight * 1.75f);
        canvasRect.localPosition = new Vector3(0, 1.75f, 0);
        canvasRect.localRotation = Quaternion.Euler(90, 0, 0);

        GameObject containerGO = new GameObject("CountContainer");
        containerGO.transform.SetParent(canvasGO.transform, false);

        var layout = containerGO.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        RectTransform containerRect = containerGO.GetComponent<RectTransform>();
        containerRect.sizeDelta = canvasRect.sizeDelta;
        containerRect.localPosition = Vector3.zero;
        containerRect.anchorMin = containerRect.anchorMax = containerRect.pivot = new Vector2(0.5f, 0.5f);

        // Текст
        GameObject textGO = new GameObject("CountText");
        textGO.transform.SetParent(containerGO.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0;
        tmp.color = Color.white;

        int blockCount = CountBlocks(blockParent.shape);
        tmp.text = "+" + blockCount.ToString();

        RectTransform textRect = tmp.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x * 0.5f, canvasRect.sizeDelta.y);
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(1, 0.5f);

        blockParent.bulletsCountText = tmp;

        // Иконка
        GameObject iconGO = new GameObject("BulletIcon");
        iconGO.transform.SetParent(containerGO.transform, false);
        var iconImage = iconGO.AddComponent<Image>();
        iconImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Project/UI/Bullet_Icon.png");
        iconImage.preserveAspect = true;

        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x * 0.5f, canvasRect.sizeDelta.y);
        iconRect.anchorMin = new Vector2(0, 0);
        iconRect.anchorMax = new Vector2(1, 1);
        iconRect.pivot = new Vector2(0, 0.5f);
    }

    private static int CountBlocks(List<RowData> shape)
    {
        int count = 0;
        foreach (var row in shape)
            foreach (int cell in row.row)
                if (cell == 1) count++;
        return count;
    }

    private static (int minX, int maxX, int minY, int maxY) GetBoundsFromShapeRows(List<string> shapeRows)
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

    private static string GetTargetFolder(List<string> shapeRows)
    {
        bool isRectangle = true;
        var bounds = GetBoundsFromShapeRows(shapeRows);
        for (int y = bounds.minY; y <= bounds.maxY; y++)
            for (int x = bounds.minX; x <= bounds.maxX; x++)
                if (shapeRows[y][x] != '1') isRectangle = false;

        string target = isRectangle ? $"{BASEFOLDER}/Nx{shapeRows.Count}" : $"{BASEFOLDER}/Specific";
        if (!AssetDatabase.IsValidFolder(target))
            AssetDatabase.CreateFolder(BASEFOLDER, isRectangle ? $"Nx{shapeRows.Count}" : "Specific");
        return target;
    }

    private static void SavePrefabAsset(GameObject shapeRoot, List<string> shapeRows, int width, int height, string folder)
    {
        string shapeHash = GetShapeHash(shapeRows);
        string prefabName = $"{width}x{height}_{shapeHash}.prefab";
        string prefabPath = $"{folder}/{prefabName}";

        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            var oldBlockParent = existingPrefab.GetComponent<BlockParent>();
            var newBlockParent = shapeRoot.GetComponent<BlockParent>();
            if (oldBlockParent != null && newBlockParent != null)
                newBlockParent.nextRangBlock = oldBlockParent.nextRangBlock;

            PrefabUtility.SaveAsPrefabAssetAndConnect(shapeRoot, prefabPath, InteractionMode.UserAction);
            Debug.Log($"Префаб <color=yellow>обновлён</color>: {prefabName}");
        }
        else
        {
            PrefabUtility.SaveAsPrefabAsset(shapeRoot, prefabPath);
            Debug.Log($"Префаб <color=green>создан</color>: {prefabName}");
        }

        AssetDatabase.SaveAssets();
    }

    private static string GetShapeHash(List<string> shape)
    {
        string shapeString = string.Concat(shape);
        using MD5 md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(shapeString));
        StringBuilder sb = new();
        foreach (byte b in hashBytes) sb.Append(b.ToString("x2"));
        return sb.ToString()[..6];
    }
    #endregion

    #region Mass Regeneration
    private void RegenerateAllPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { BASEFOLDER });
        int count = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            BlockParent bp = prefab.GetComponent<BlockParent>();
            if (bp != null && bp.shape != null)
            {
                // Преобразуем shapeData обратно в shapeRows
                List<string> shapeRowsFromPrefab = new();
                for (int i = bp.shape.Count - 1; i >= 0; i--)
                {
                    var rowData = bp.shape[i];
                    StringBuilder sb = new();
                    foreach (int val in rowData.row)
                        sb.Append(val == 1 ? '1' : '0');
                    shapeRowsFromPrefab.Add(sb.ToString());
                }


                GeneratePrefab(shapeRowsFromPrefab, cubePrefab);
                count++;
            }
        }

        EditorUtility.DisplayDialog("Готово", $"Перегенерировано {count} префабов", "OK");
    }
    #endregion
}
