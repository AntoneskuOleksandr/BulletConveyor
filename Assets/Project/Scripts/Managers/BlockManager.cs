using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [SerializeField] private MergePlatformsManager platformsManager;
    [SerializeField] private GameObject[] blocks;
    [SerializeField] private float baseBlockPrice = 100f;
    [SerializeField] private float priceMultiplier = 1.25f;

    public float BlockPrice => currentBlockPrice;

    private int buyedBlockCount = 0;
    private int maxBlockRank = 0;
    private float currentBlockPrice;

    private void Awake()
    {
        // при старте цена равна базовой
        currentBlockPrice = baseBlockPrice;
    }

    public void CreateBlock(bool isFreeBlock = false)
    {
        int blockToCreateIndex = 0;

        if (isFreeBlock)
            blockToCreateIndex = maxBlockRank;

        int platformIndex = platformsManager.GetFreePlatformIndex();

        if (platformIndex == -1)
        {
            Debug.LogWarning("No free platform available to create a block.");
            return;
        }
        if (blockToCreateIndex < 0 || blockToCreateIndex >= blocks.Length)
        {
            Debug.LogError($"Invalid block index: {blockToCreateIndex}. Must be between 0 and {blocks.Length - 1}.");
            return;
        }
        if (blocks[blockToCreateIndex] == null)
        {
            Debug.LogError($"Block prefab at index {blockToCreateIndex} is null. Please assign a valid prefab.");
            return;
        }

        BlockParent parent = Instantiate(
            blocks[blockToCreateIndex],
            platformsManager.GetPlatformPositionByIndex(platformIndex),
            Quaternion.identity,
            this.transform
        ).GetComponent<BlockParent>();

        platformsManager.SetPlatformBusy(platformIndex);
        parent.PlaceOnPlatform(platformsManager.GetPlatformPositionByIndex(platformIndex), platformIndex);

        if (!isFreeBlock)
        {
            buyedBlockCount++;
            IncreaseBlockPrice();
        }

        parent.bulletsToGenerate += 0;
        parent.rank = blockToCreateIndex;
    }

    public void IncreaseBlockPrice()
    {
        currentBlockPrice = Mathf.Ceil(currentBlockPrice * priceMultiplier);
    }

    public void MergeBlocks(BlockParent block1, BlockParent block2)
    {
        Debug.Log($"Merging blocks: {block1.name} and {block2.name}");
        if (block1 == null || block2 == null)
        {
            if (block1 == null)
                Debug.LogError("Block 1 is null. Cannot merge.");
            if (block2 == null)
                Debug.LogError("Block 2 is null. Cannot merge.");
            return;
        }

        BlockParent newBlock = Instantiate(
            block1.nextRangBlock,
            block2.transform.position,
            Quaternion.identity
        ).GetComponent<BlockParent>();

        newBlock.blockData.platform = block2.blockData.platform;
        newBlock.rank = block2.rank + 1;

        if (block1.blockData.platform != -1)
            platformsManager.SetPlatformFree(block1.blockData.platform);

        if (newBlock.rank > maxBlockRank)
            maxBlockRank = newBlock.rank;

        Destroy(block1.gameObject);
        Destroy(block2.gameObject);
    }

    public bool CanCreateBlock()
    {
        return platformsManager.IsFreePlatform();
    }
}
