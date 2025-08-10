using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ShapePrefabCreator : EditorWindow
{
    public GameObject cubePrefab;
    public List<string> shapeRows = new() { "111", "010" };
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

        // Снизу вверх — совпадение порядка в сцене и в данных
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

        string targetFolder = GetTargetFolder(IsShapeRectangular(bounds));
        SavePrefab(shapeRoot, shapeWidth, shapeHeight, targetFolder);

        DestroyImmediate(shapeRoot);
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
