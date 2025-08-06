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
    private List<Tile> availableTiles = new List<Tile>();

    private Tile previousTile;

    private void Start()
    {
        tiles = new Tile[rows, columns];
        tiles = tileGenerator.GetAllTiles();
        onTilesGenerated.Invoke();
    }

    public Tile GetTile(Vector2Int index)
    {
        if (index.x >= 0 && index.x < rows && index.y >= 0 && index.y < columns)
        {
            return tiles[index.x, index.y];
        }

        Debug.LogWarning("There are no tiles with " + index + " index");
        return null;
    }

    public void HighLightTiles(List<Tile> highLightTiles, bool highLight = true)
    {
        if (highLightTiles != null)
        {
            foreach (Tile tile in highLightTiles)
            {
                if (highLight)
                {
                    tile.HighLight();
                }
                else
                {
                    tile.SetDefaultColor();
                }
            }
        }
    }

    public Tile[,] GetAllTiles()
    {
        if (tiles == null)
        {
            Debug.LogError("Tiles have not been generated yet. Please call GenerateGrid first.");
            return null;
        }

        return tiles;
    }

    public List<Tile> GetAvailableTiles(BlockParent blockParent, Tile tile)
    {
        if (tile == previousTile)
            return availableTiles;

        availableTiles.Clear();

        for (int x = 0; x < blockParent.shape.Count; x++)
        {
            for (int y = 0; y < blockParent.shape[0].row.Length; y++)
            {
                if (x + tile.indexInMatrix.x >= tiles.GetLength(0) || y + tile.indexInMatrix.y >= tiles.GetLength(1))
                {
                    availableTiles.Clear();
                    return null;
                }
                Tile currentTile = tiles[x + tile.indexInMatrix.x, y + tile.indexInMatrix.y];

                if (currentTile.isAvailable && blockParent.shape[x].row[y] == 1)
                {
                    availableTiles.Add(currentTile);
                }
            }
        }

        previousTile = tile;

        return availableTiles;
    }
}

