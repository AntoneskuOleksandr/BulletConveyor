using System.Runtime.CompilerServices;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public Vector3 offSet;
    private Tile[,] tiles;
    private float cellSize;

    public void GenerateGrid(int rows, int columns)
    {
        cellSize = cellPrefab.transform.localScale.x;
        tiles = new Tile[rows, columns];

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize) + offSet;

                tiles[x, y] = Instantiate(cellPrefab, position, Quaternion.identity, transform).GetComponent<Tile>();
                tiles[x, y].indexInMatrix = new Vector2Int(x, y);
                tiles[x, y].name = string.Format("X:{0}, Y:{1}", x, y);
            }
        }
    }

    public Tile[,] GetTiles()
    {
        return tiles;
    }
}
