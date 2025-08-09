using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class RowData
{
    public int[] row;
}

public class BlockParent : MonoBehaviour
{
    [SerializeField] private float DraggSpeed = 7f;
    [SerializeField] private TMP_Text bulletsCount;

    public int scaleX = 1;
    public int scaleZ = 1;

    private Vector3 desiredPosition;
    private Vector3 previousPosition;

    public int bulletsToGenerate;
    public bool isOnBoard;
    private bool previousIsOnBoard;

    public GameObject bulletPrefab;
    public GameObject nextRangBlock;
    public int rank;
    public int platform;
    private int previousPlatform;
    private int originalLayer;

    public List<RowData> shape = new List<RowData>();

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

    private AbilityManager abilityManager;

    private void Awake()
    {
        originalLayer = LayerMask.NameToLayer("Block");

        abilityManager = AbilityManager.Instance;
        abilityManager.OnBulletToGenerateAbilitySelected.AddListener(AddBulletToGenerate);

        desiredPosition = transform.position;
        previousPosition = transform.position;

        if (bulletsCount != null)
            bulletsCount.text = "+" + bulletsToGenerate.ToString();
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * DraggSpeed);
    }

    public void SetPosition(Vector3 position)
    {
        desiredPosition = position;
    }

    public void MoveToPreviousPosition()
    {
        desiredPosition = previousPosition;
    }

    private void AddBulletToGenerate()
    {
        bulletsToGenerate++;
        bulletsCount.text = "+" + bulletsToGenerate.ToString();
    }

    public void StartDragging()
    {
        SafePreviousState();

        ChangeLayerRecursively(transform, LayerMask.NameToLayer("DraggingBlock"));

        isOnBoard = false; // блок не на доске
        platform = -1; // блок не на платформе

        foreach (Tile tile in MyTiles)
        {
            tile.isAvailable = true; // освобождаем занятые тайлы
        }
    }

    public void PlaceOnBoard(List<Tile> tiles, Vector3 position)
    {
        SafePreviousState(); // сохраняем предыдущие тайлы и позицию

        RestoreLayer();

        platform = -1; // блок не на платформе
        isOnBoard = true; // блок на доске
        MyTiles = tiles; // обновляем занятые тайлы

        foreach (Tile tile in MyTiles)
        {
            tile.isAvailable = false; // блокируем занятые тайлы
        }

        SetPosition(position); // устанавливаем позицию блока
    }

    public void PlaceOnPlatform(Vector3 position, int platformIndex)
    {
        RestoreLayer();

        MyTiles = new List<Tile>(); // очищаем занятые тайлы
        isOnBoard = false; // блок не на доске
        platform = platformIndex; // устанавливаем платформу

        SetPosition(position);
    }

    private void SafePreviousState()
    {
        if (myTiles == null)
            myTiles = new List<Tile>();

        myPreviousTiles = new List<Tile>(myTiles); // сохраняем предыдущие тайлы
        previousPosition = desiredPosition; // сохраняем предыдущую позицию
        previousIsOnBoard = isOnBoard; // сохраняем состояние на доске
        previousPlatform = platform; // сохраняем предыдущую платформу

    }

    public void ReturnToPreviousState()
    {
        // Возвращаемся к предыдущему состоянию
        desiredPosition = previousPosition;
        MyTiles = myPreviousTiles;
        isOnBoard = previousIsOnBoard;
        platform = previousPlatform;

        RestoreLayer();
    }

    public bool IsWasOnPlatform()
    {
        return platform != -1;
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
}
