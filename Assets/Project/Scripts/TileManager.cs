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

    private void Start()
    {
        tiles = new Tile[rows, columns];
        tiles = tileGenerator.GetTiles();
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
}

