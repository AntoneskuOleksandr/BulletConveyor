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
