using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [SerializeField] private MergePlatformsManager platformsManager;
    [SerializeField] private GameObject[] blocks;

    public void CreateBlock(int index = 0)
    {
        int platformIndex = platformsManager.GetFreePlatformIndex();

        if (platformIndex == -1)
        {
            Debug.LogWarning("No free platform available to create a block.");
            return;
        }
        if (index < 0 || index >= blocks.Length)
        {
            Debug.LogError($"Invalid block index: {index}. Must be between 0 and {blocks.Length - 1}.");
            return;
        }
        if (blocks[index] == null)
        {
            Debug.LogError($"Block prefab at index {index} is null. Please assign a valid prefab.");
            return;
        }

        BlockParent parent = Instantiate(blocks[index], 
            platformsManager.GetPlatformPositionByIndex(platformIndex), Quaternion.identity, this.transform).GetComponent<BlockParent>();

        platformsManager.SetPlatformBusy(platformIndex);

        parent.PlaceOnPlatform(platformsManager.GetPlatformPositionByIndex(platformIndex), platformIndex);

        parent.bulletsToGenerate += 0;
        parent.rank = index;

        return;
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

        BlockParent newBlock = Instantiate(block1.nextRangBlock, block2.transform.position, Quaternion.identity).GetComponent<BlockParent>();
        newBlock.blockData.platform = block2.blockData.platform;
        newBlock.rank = block2.rank + 1;

        if (block1.blockData.platform != -1)
            platformsManager.SetPlatformFree(block1.blockData.platform);

        Destroy(block1.gameObject);
        Destroy(block2.gameObject);
    }

    public bool CanCreateBlock()
    {
        if (platformsManager.IsFreePlatform())
            return true;

        return false;
    }
}
