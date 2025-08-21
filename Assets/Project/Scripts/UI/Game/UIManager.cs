using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class UIManager : MonoBehaviour
{
    [SerializeField] private Button buyBlockByAdButton;

    private BlockManager _blockManager;

    [Inject]
    public void Construct(BlockManager blockManager)
    {
        _blockManager = blockManager;
    }

    private void Awake()
    {
        if (buyBlockByAdButton != null)
        {
            buyBlockByAdButton.onClick.AddListener(OnBuyBlockByAdButtonClicked);
        }
    }

    private void OnBuyBlockByAdButtonClicked()
    {
        Debug.Log("Buy Block by Ad button clicked.");
        if (_blockManager != null)
        {
            _blockManager.CreateBlock(0); // Assuming 0 is the index of the block to create
        }
        else
        {
            Debug.LogError("BlockManager is not initialized. Cannot create block.");
        }
    }
}