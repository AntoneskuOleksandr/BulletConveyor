using System.Collections;
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
}

public class BlockParent : MonoBehaviour
{
    [SerializeField] private float DraggSpeed = 7f;
    public TMP_Text bulletsCountText;

    public int scaleX = 1;
    public int scaleZ = 1;

    public BlockState blockState = BlockState.OnPlatform;
    public BlockState blockPreviousState = BlockState.OnPlatform;

    public BlockData blockData = new BlockData();
    public BlockData blockPreviousData = new BlockData();

    private Vector3 desiredPosition;

    public int bulletsToGenerate;

    public GameObject nextRangBlock;
    public int rank;
    private int originalLayer;

    public List<RowData> shape = new List<RowData>();

    public UnityEvent onDragStart;

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

            myPreviousTiles = new List<Tile>(myTiles);
            myTiles = value;
        }
    }

    private List<Tile> myTiles = new List<Tile>();
    private List<Tile> myPreviousTiles = new List<Tile>();

    private void Awake()
    {
        originalLayer = LayerMask.NameToLayer("Block");

        desiredPosition = transform.position;

        if (bulletsCountText != null)
            bulletsCountText.text = "+" + bulletsToGenerate.ToString();
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * DraggSpeed);
    }

    public void SetPosition(Vector3 position)
    {
        desiredPosition = position;
    }

    private void AddBulletToGenerate()
    {
        bulletsToGenerate++;
        bulletsCountText.text = "+" + bulletsToGenerate.ToString();
    }

    public void StartDragging()
    {
        onDragStart?.Invoke();

        SavePreviousData();

        ChangeLayerRecursively(transform, LayerMask.NameToLayer("DraggingBlock"));

        blockData.isOnBoard = false; // блок не на доске
        blockData.platform = -1; // блок не на платформе

        foreach (Tile tile in MyTiles)
        {
            tile.isAvailable = true; // освобождаем занятые тайлы
        }

        MyTiles = new List<Tile>(); // очищаем занятые тайлы
    }

    public void PlaceOnBoard(List<Tile> tiles, Vector3 position)
    {
        SavePreviousData();
        blockState = BlockState.OnBoard;

        RestoreLayer();

        blockData.platform = -1; // блок не на платформе
        blockData.isOnBoard = true; // блок на доске
        MyTiles = tiles; // обновляем занятые тайлы

        foreach (Tile tile in MyTiles)
        {
            tile.isAvailable = false; // блокируем занятые тайлы
        }

        SetPosition(position); // устанавливаем позицию блока
    }

    public void PlaceOnPlatform(Vector3 position, int platformIndex)
    {
        SavePreviousData();
        blockState = BlockState.OnPlatform;

        RestoreLayer();

        MyTiles = new List<Tile>(); // очищаем занятые тайлы
        blockData.isOnBoard = false; // блок не на доске
        blockData.platform = platformIndex; // устанавливаем платформу

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
