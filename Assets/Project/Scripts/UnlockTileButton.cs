using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UnlockTileButton : MonoBehaviour
{
    public Button unlockButton;
    public TileManager tileManager;
    public List<Vector2Int> myTiles;
    public int unlockPrice = 100;
    public bool useGameCurrency = false;
    public bool isTilesForAdd = false;

    public CoinManager coinManager;

    private TMP_Text priceText;

    private void Awake()
    {
        if (!isTilesForAdd)
        {
            priceText = GetComponentInChildren<TMP_Text>();
            if (useGameCurrency)
                priceText.text = unlockPrice.ToString() + " COINS";
            else
                priceText.text = unlockPrice.ToString() + "$";
        }

        unlockButton.onClick.AddListener(() => UnlockTiles());
        tileManager.onTilesGenerated.AddListener(LockTiles);
    }

    private void LockTiles()
    {
        foreach (Vector2Int index in myTiles)
        {
            Tile tile = tileManager.GetTile(index);

            if (tile != null)
                tile.isAvailable = false;
        }
    }

    private void UnlockTiles()
    {
        foreach (Vector2Int index in myTiles)
        {
            Tile tile = tileManager.GetTile(index);
            if (tile != null)
            {
                tile.isAvailable = true;
            }
        }

        if (isTilesForAdd)
        {
            GameManager.Instance.ShowAdd();
        }

        else if (useGameCurrency)
        {
            float coins = PlayerPrefs.GetFloat("Coins");
            PlayerPrefs.SetFloat("Coins", coins - unlockPrice);
        }
        else
        {
            coinManager.Coins -= unlockPrice;
        }

        gameObject.SetActive(false);
    }
}
