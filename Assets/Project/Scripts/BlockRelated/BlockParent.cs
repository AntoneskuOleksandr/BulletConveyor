using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class RowData
{
    public int[] row;
}

public enum BlockState
{
    OnPlatform,
    OnBoard
}

public class BlockData
{
    public bool isOnBoard;
    public int platform;
    public Vector2Int topLeftIndex = Vector2Int.zero;
}

public class BlockParent : MonoBehaviour
{
    [SerializeField] private float DraggSpeed = 7f;

    public bool CanDrag = true;

    public TMP_Text ProjectilesCountText;

    public int scaleX = 1;
    public int scaleZ = 1;

    public BlockState blockState = BlockState.OnPlatform;
    public BlockState blockPreviousState = BlockState.OnPlatform;

    public BlockData blockData = new BlockData();
    public BlockData blockPreviousData = new BlockData();

    private Vector3 desiredPosition;

    public int projectilesToGenerate;

    public GameObject nextRangBlock;
    public int rank;
    private int originalLayer;

    public List<RowData> shape = new List<RowData>();

    public UnityEvent onDragFromBoard;

    public List<Tile> MyTiles
    {
        get { return myTiles; }
        set
        {
            if (value == null)
            {
                myTiles = new List<Tile>();
                return;
            }

            myTiles = value;
        }
    }

    private List<Tile> myTiles = new List<Tile>();

    private void Awake()
    {
        originalLayer = LayerMask.NameToLayer("Block");

        desiredPosition = transform.position;

        if (ProjectilesCountText != null)
            ProjectilesCountText.text = "+" + projectilesToGenerate.ToString();
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * DraggSpeed);
    }

    public void SetPosition(Vector3 position)
    {
        desiredPosition = position;
    }

    private void AddProjectileToGenerate()
    {
        projectilesToGenerate++;
        ProjectilesCountText.text = "+" + projectilesToGenerate.ToString();
    }

    public void StartDragging()
    {
        SavePreviousData();

        if (blockState.Equals(BlockState.OnBoard))
            onDragFromBoard?.Invoke();

        ChangeLayerRecursively(transform, LayerMask.NameToLayer("DraggingBlock"));

        blockData.isOnBoard = false;
        blockData.platform = -1;

        foreach (Tile tile in MyTiles)
        {
            tile.isAvailable = true;
        }

        MyTiles = new List<Tile>();
    }

    public void PlaceOnBoard(List<Tile> tiles, Vector3 position)
    {
        SavePreviousData();
        blockState = BlockState.OnBoard;

        RestoreLayer();

        blockData.platform = -1;
        blockData.isOnBoard = true;
        MyTiles = tiles;

        foreach (Tile tile in MyTiles)
        {
            tile.isAvailable = false;
        }

        SetPosition(position);
    }

    public void PlaceOnPlatform(Vector3 position, int platformIndex)
    {
        SavePreviousData();
        blockState = BlockState.OnPlatform;

        RestoreLayer();

        MyTiles = new List<Tile>();
        blockData.isOnBoard = false;
        blockData.platform = platformIndex;

        SetPosition(position);
    }

    private void ChangeLayerRecursively(Transform target, int layer)
    {
        target.gameObject.layer = layer;
        foreach (Transform child in target)
        {
            ChangeLayerRecursively(child, layer);
        }
    }

    private void RestoreLayer()
    {
        ChangeLayerRecursively(transform, originalLayer);
    }

    private void SavePreviousData()
    {
        blockPreviousData.isOnBoard = blockData.isOnBoard;
        blockPreviousData.platform = blockData.platform;

        blockPreviousState = blockState;
    }
}
