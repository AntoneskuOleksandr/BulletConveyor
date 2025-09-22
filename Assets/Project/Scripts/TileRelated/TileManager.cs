using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TileManager : MonoBehaviour
{
    public TileGenerator tileGenerator;
    public UnityEvent onTilesGenerated;

    public int rows = 6;
    public int columns = 6;

    private Tile[,] tiles;
    private Vector3 tileOrigin;
    private float tileSize;

    public float CellSize => tileSize; // <— добавлено

    public void Initialize()
    {
        tiles = tileGenerator.GetAllTiles();

        if (tiles[0, 0] != null)
        {
            tileOrigin = tiles[0, 0].transform.position;
            tileSize = tiles[0, 0].transform.localScale.x;
        }

        onTilesGenerated.Invoke();
    }

    // -------------------------------
    // Основные утилиты
    // -------------------------------

    public Tile GetTileAt(Vector2Int index)
    {
        if (IsInsideBoard(index))
            return tiles[index.x, index.y];

        Debug.LogWarning($"Tile index {index} is out of bounds!");
        return null;
    }

    public bool IsInsideBoard(Vector2Int index)
    {
        return index.x >= 0 && index.x < rows && index.y >= 0 && index.y < columns;
    }

    public Vector2Int WorldToTile(Vector3 worldPos)
    {
        Vector3 offset = worldPos - tileOrigin;
        int x = Mathf.RoundToInt(offset.x / tileSize);
        int y = Mathf.RoundToInt(offset.z / tileSize);
        return new Vector2Int(x, y);
    }

    public Vector2 WorldToTileFloat(Vector3 worldPos) // <— добавлено
    {
        Vector3 offset = worldPos - tileOrigin;
        float x = offset.x / tileSize;
        float y = offset.z / tileSize;
        return new Vector2(x, y);
    }

    public Vector3 TileToWorld(Vector2Int index)
    {
        return tileOrigin + new Vector3(index.x * tileSize, 0, index.y * tileSize);
    }

    public Tile[,] GetAllTiles() => tiles;

    public List<Tile> GetAllTilesList()
    {
        List<Tile> list = new List<Tile>();
        foreach (Tile t in tiles)
            list.Add(t);
        return list;
    }

    public void HighLightTiles(List<Tile> tilesToHighlight, bool highlight = true)
    {
        if (tilesToHighlight == null) return;
        foreach (Tile tile in tilesToHighlight)
        {
            if (highlight)
                tile.HighLight();
            else
                tile.SetDefaultColor();
        }
    }

    // -------------------------------
    // Подсчёт pivot и доступных тайлов
    // -------------------------------

    /// <summary>
    /// Находит pivot фигуры как центр bounding box (учитывая нули)
    /// </summary>
    private Vector2 GetBoundingBoxPivot(BlockParent blockParent)
    {
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        for (int y = 0; y < blockParent.shape.Count; y++)
        {
            for (int x = 0; x < blockParent.shape[y].row.Length; x++)
            {
                if (blockParent.shape[y].row[x] == 1)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        return new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
    }

    /// <summary>
    /// Проверяет и возвращает список доступных тайлов для фигуры, опираясь на bounding box pivot
    /// </summary>
    public List<Tile> GetAvailableTiles(BlockParent blockParent, Vector2Int targetTile)
    {
        List<Tile> availableTiles = new List<Tile>();

        Vector2 pivotOffset = GetBoundingBoxPivot(blockParent);

        // targetTile — это куда должен встать pivot фигуры
        for (int y = 0; y < blockParent.shape.Count; y++)
        {
            for (int x = 0; x < blockParent.shape[y].row.Length; x++)
            {
                if (blockParent.shape[y].row[x] == 1)
                {
                    Vector2Int boardIndex = new Vector2Int(
                        targetTile.x + (x - Mathf.RoundToInt(pivotOffset.x)),
                        targetTile.y + (y - Mathf.RoundToInt(pivotOffset.y))
                    );

                    if (!IsInsideBoard(boardIndex))
                        return null; // фигура выходит за границы

                    Tile tile = GetTileAt(boardIndex);
                    if (!tile.isAvailable)
                        return null; // занятый тайл

                    availableTiles.Add(tile);
                }
            }
        }

        return availableTiles;
    }
}
