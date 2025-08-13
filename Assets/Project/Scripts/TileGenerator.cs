using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize;
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

        Debug.Log("Grid generated with size: " + rows + "x" + columns);
    }

    public Tile[,] GetAllTiles()
    {
        if (tiles == null)
        {
            Debug.LogWarning("Tiles have not been generated yet. Calling GenerateGrid.");
            GenerateGrid(gridSize.x, gridSize.y);
        }

        return tiles;
    }
}
