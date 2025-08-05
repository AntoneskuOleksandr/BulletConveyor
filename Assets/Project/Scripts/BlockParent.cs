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

    public Vector3 desiredPosition;

    public int bulletsToGenerate;
    public bool isOnBoard;
    public GameObject bulletPrefab;
    public GameObject nextRangBlock;
    public int rank;
    public int platform;

    public Tile[] myTiles;

    public Vector3 previousPosition;

    private AbilityManager abilityManager;

    public List<RowData> shape = new List<RowData>();

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
}
