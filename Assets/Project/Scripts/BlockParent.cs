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

    public List<RowData> shape = new List<RowData>();

    public List<Tile> MyTiles
    {
        get { return myTiles; }
        set
        {
            myPreviousTiles = new List<Tile>(myTiles);
            myTiles = value;
        }
    }

    private List<Tile> myTiles;
    private List<Tile> myPreviousTiles;

    private AbilityManager abilityManager;

    private void Start()
    {
        abilityManager = AbilityManager.Instance;
        abilityManager.OnBulletToGenerateAbilitySelected.AddListener(AddBulletToGenerate);

        desiredPosition = transform.position;
        previousPosition = transform.position;

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

        isOnBoard = false; // блок не на доске

        foreach (Tile tile in MyTiles)
        {
            tile.isAvailable = true; // освобождаем занятые тайлы
        }
    }

    public void PlaceOnBoard(List<Tile> tiles, Vector3 position)
    {
        SafePreviousState(); // сохраняем предыдущие тайлы и позицию

        platform = -1; // блок не на платформе
        isOnBoard = true; // блок на доске
        MyTiles = tiles; // обновляем занятые тайлы

        foreach (Tile tile in MyTiles)
        {
            tile.isAvailable = false; // освобождаем занятые тайлы
        }

        SetPosition(position); // устанавливаем позицию блока
    }

    public void PlaceOnPlatform(Vector3 position, int platformIndex)
    {
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
    }

    public void ReturnToPreviousState()
    {
        desiredPosition = previousPosition;
        MyTiles = myPreviousTiles;
        isOnBoard = previousIsOnBoard;
    }

}
