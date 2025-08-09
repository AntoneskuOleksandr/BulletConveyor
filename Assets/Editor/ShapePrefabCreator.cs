using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ShapePrefabCreator : EditorWindow
{
    public GameObject cubePrefab;
    public List<string> shapeRows = new List<string>()
    {
        "111",
        "010"
    };
    public float cubeSize = 1.75f;

    private string baseFolder = "Assets/Project/Prefabs/Blocks";

    [MenuItem("Tools/Create Shape Prefab")]
    public static void ShowWindow()
    {
        GetWindow<ShapePrefabCreator>("Shape Prefab Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Настройки генерации фигуры", EditorStyles.boldLabel);

        cubePrefab = (GameObject)EditorGUILayout.ObjectField("Cube Prefab", cubePrefab, typeof(GameObject), false);
        cubeSize = EditorGUILayout.FloatField("Cube Size", cubeSize);

        EditorGUILayout.LabelField("Shape Rows (use '1' for cube, '0' for empty):");

        int toRemoveIndex = -1;
        for (int i = 0; i < shapeRows.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            shapeRows[i] = EditorGUILayout.TextField(shapeRows[i]);

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                toRemoveIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (toRemoveIndex >= 0)
            shapeRows.RemoveAt(toRemoveIndex);

        if (GUILayout.Button("+ Add Row"))
        {
            shapeRows.Add("");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate and Save Prefab"))
        {
            if (cubePrefab == null)
            {
                EditorUtility.DisplayDialog("Ошибка", "Пожалуйста, укажи Cube Prefab!", "ОК");
                return;
            }

            if (shapeRows.Count == 0)
            {
                EditorUtility.DisplayDialog("Ошибка", "Shape Rows пусты!", "ОК");
                return;
            }

            if (!ValidateShapeRows())
            {
                EditorUtility.DisplayDialog("Ошибка", "Все строки в Shape Rows должны быть одинаковой длины и содержать только '0' или '1'!", "ОК");
                return;
            }

            CreateShapePrefab();
        }
    }

    private bool ValidateShapeRows()
    {
        if (shapeRows.Count == 0) return false;
        int len = shapeRows[0].Length;
        foreach (var row in shapeRows)
        {
            if (row.Length != len) return false;
            foreach (char c in row)
            {
                if (c != '0' && c != '1') return false;
            }
        }
        return true;
    }

    private void CreateShapePrefab()
    {
        GameObject shapeRoot = new GameObject("GeneratedShape");

        int rows = shapeRows.Count;
        int cols = shapeRows[0].Length;

        // Находим минимальный ограничивающий прямоугольник
        int minX = cols, maxX = -1, minY = rows, maxY = -1;

        for (int y = 0; y < rows; y++)
        {
            string row = shapeRows[y];
            for (int x = 0; x < cols; x++)
            {
                if (row[x] == '1')
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (maxX == -1 || maxY == -1)
        {
            EditorUtility.DisplayDialog("Ошибка", "Фигура не содержит ни одного куба!", "ОК");
            DestroyImmediate(shapeRoot);
            return;
        }

        int shapeWidth = maxX - minX + 1;
        int shapeHeight = maxY - minY + 1;

        float offsetX = (shapeWidth - 1) * cubeSize / 2f;
        float offsetZ = (shapeHeight - 1) * cubeSize / 2f;

        // Создаём кубы только в минимальном ограничивающем прямоугольнике
        for (int y = minY; y <= maxY; y++)
        {
            string row = shapeRows[y];
            for (int x = minX; x <= maxX; x++)
            {
                if (row[x] == '1')
                {
                    Vector3 localPos = new Vector3((x - minX) * cubeSize - offsetX, 0, -(y - minY) * cubeSize + offsetZ);
                    GameObject cubeInstance = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab, shapeRoot.transform);
                    cubeInstance.transform.localPosition = localPos;
                }
            }
        }

        bool isRectangle = IsShapeRectangular(shapeRows);

        string targetFolder;

        if (isRectangle)
        {
            targetFolder = $"{baseFolder}/Nx{shapeHeight}";
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                targetFolder = $"{baseFolder}/Specific";
                if (!AssetDatabase.IsValidFolder(targetFolder))
                    AssetDatabase.CreateFolder(baseFolder, "Specific");
            }
        }
        else
        {
            targetFolder = $"{baseFolder}/Specific";
            if (!AssetDatabase.IsValidFolder(targetFolder))
                AssetDatabase.CreateFolder(baseFolder, "Specific");
        }

        string prefabName = $"{shapeWidth}x{shapeHeight}.prefab";
        string prefabPath = $"{targetFolder}/{prefabName}";

        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
            AssetDatabase.DeleteAsset(prefabPath);

        PrefabUtility.SaveAsPrefabAsset(shapeRoot, prefabPath);
        AssetDatabase.SaveAssets();

        DestroyImmediate(shapeRoot);

        EditorUtility.DisplayDialog("Успех", $"Префаб сохранён по пути:\n{prefabPath}", "ОК");
    }

    private bool IsShapeRectangular(List<string> shape)
    {
        int rows = shape.Count;
        int cols = shape[0].Length;

        int minX = cols, maxX = -1, minY = rows, maxY = -1;

        // Найти минимальный ограничивающий прямоугольник по заполненным блокам
        for (int y = 0; y < rows; y++)
        {
            string row = shape[y];
            for (int x = 0; x < cols; x++)
            {
                if (row[x] == '1')
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (maxX == -1 || maxY == -1)
            return false;

        // Проверить, что внутри найденного прямоугольника нет пустых клеток
        for (int y = minY; y <= maxY; y++)
        {
            string row = shape[y];
            for (int x = minX; x <= maxX; x++)
            {
                if (row[x] != '1')
                    return false;
            }
        }

        return true;
    }
}
