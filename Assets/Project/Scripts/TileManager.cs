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
    public List<Tile> availableTiles = new List<Tile>();

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
        availableTiles.Clear();

        for (int i = 0; i < blockParent.shape.Count; i++)
        {
            for (int j = 0; j < blockParent.shape[0].row.Length; j++)
            {
                if (tile.indexInMatrix.x + j >= tiles.GetLength(0) || tile.indexInMatrix.y + i >= tiles.GetLength(1))
                {
                    availableTiles.Clear();
                    return null;
                }

                if (blockParent.shape[i].row[j] == 1)
                {
                    if (!tiles[tile.indexInMatrix.x + j, tile.indexInMatrix.y + i].isAvailable)
                    {
                        availableTiles.Clear();
                        return null;
                    }
                    else
                    {
                        availableTiles.Add(tiles[tile.indexInMatrix.x + j, tile.indexInMatrix.y + i]);
                    }
                }
            }
        }

        return availableTiles;
    }

    public Tile GetTileUnderBlock(Block block)
    {
        Ray blockRay = new Ray(block.transform.position, Vector3.down);

        if (Physics.Raycast(blockRay, out RaycastHit hit, 5f))
        {
            GameObject hitGO = hit.transform.gameObject;

            if (hitGO.layer == LayerMask.NameToLayer("Tile"))
            {
                Tile tile = hitGO.GetComponent<Tile>();

                if (tile != null)
                {
                    return tile;
                }
            }
        }

        return null;
    }

    public List<Tile> CheckAvailableTiles(Block block, BlockParent blockParent)
    {
        Ray blockRay = new Ray(block.transform.position, Vector3.down);

        Debug.DrawRay(blockRay.origin, blockRay.direction * 5, Color.red);

        if (GetTileUnderBlock(block) != null)
            return GetAvailableTiles(blockParent, GetTileUnderBlock(block));

        return null;
    }
}

