using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;
    [SerializeField] private MergePlatformsManager platformsManager;
    [SerializeField] private GameObject[] blocks;

    private void Awake()
    {
        Instance = this;
    }

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
            platformsManager.GetPlatformPositionByIndex(platformIndex), Quaternion.identity).GetComponent<BlockParent>();

        platformsManager.SetPlatformBusy(platformIndex);

        parent.PlaceOnPlatform(platformsManager.GetPlatformPositionByIndex(platformIndex), platformIndex);

        parent.bulletsToGenerate += 0;
        parent.rank = index;

        return;
    }

    public bool CanCreateBlock()
    {
        if (platformsManager.IsFreePlatform())
            return true;

        return false;
    }
}
