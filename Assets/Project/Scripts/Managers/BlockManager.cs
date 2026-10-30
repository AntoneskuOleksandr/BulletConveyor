using System;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [SerializeField] private MergePlatformsManager platformsManager;
    [SerializeField] private GameObject[] blocks;
    [SerializeField] private float baseBlockPrice = 100f;
    [SerializeField] private float priceMultiplier = 1.25f;

    public float BlockPrice => currentBlockPrice;
    public BlockParent LastCreatedBlock { get; private set; }
    public event Action<BlockParent> OnBlockCreated;

    private int buyedBlockCount = 0;
    private int maxBlockRank = 0;
    private float currentBlockPrice;

    private void Awake()
    {

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

        parent.projectilesToGenerate += 0;
        parent.rank = blockToCreateIndex;

        LastCreatedBlock = parent;
        OnBlockCreated?.Invoke(parent);
    }

    private void IncreaseBlockPrice()
    {
        currentBlockPrice = Mathf.Ceil(currentBlockPrice * priceMultiplier);
    }

    public void MergeBlocks(BlockParent block1, BlockParent block2)
    {
        if (block1 == null || block2 == null)
        {
            if (block1 == null)
                Debug.LogError("Block 1 is null. Cannot merge.");
            if (block2 == null)
                Debug.LogError("Block 2 is null. Cannot merge.");
            return;
        }

        if (block1 == block2 || block1.rank != block2.rank || block1.nextRangBlock == null || block2.blockData.isOnBoard)
            return;

        BlockParent newBlock = Instantiate(
            block1.nextRangBlock,
            block2.transform.position,
            Quaternion.identity,
            transform
        ).GetComponent<BlockParent>();

        LastCreatedBlock = newBlock;

        newBlock.blockData.platform = block2.blockData.platform;
        newBlock.rank = block2.rank + 1;
        newBlock.PlaceOnPlatform(block2.transform.position, block2.blockData.platform);
        GameAudio.PlayMerge();

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
