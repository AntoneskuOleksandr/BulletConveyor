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
    private readonly List<Tile> allTiles = new();

    public float CellSize => tileSize;

    public void Initialize()
    {
        tiles = tileGenerator.GetAllTiles();
        allTiles.Clear();
        foreach (var tile in tiles) allTiles.Add(tile);

        if (tiles[0, 0] != null)
        {
            tileOrigin = tiles[0, 0].transform.position;
            tileSize = tiles[0, 0].transform.localScale.x;
        }

        onTilesGenerated.Invoke();
    }

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

    public Vector2 WorldToTileFloat(Vector3 worldPos)
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
        return allTiles;
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

    public List<Tile> GetAvailableTiles(BlockParent blockParent, Vector2Int targetTile)
    {
        List<Tile> availableTiles = new List<Tile>();

        Vector2 pivotOffset = GetBoundingBoxPivot(blockParent);

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
                        return null;

                    Tile tile = GetTileAt(boardIndex);
                    if (!tile.isAvailable)
                        return null;

                    availableTiles.Add(tile);
                }
            }
        }

        return availableTiles;
    }
}
