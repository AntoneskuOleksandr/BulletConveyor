using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockDragController
{
    private BlockParent blockParent;
    private Block draggingFirstBlock;

    private Vector3 dragOffset;
    private float blockYOffset = 0.925f;
    private TileManager tileManager;
    private MergePlatformsManager mergePlatformsManager;
    private BlockManager blockManager;
    private Camera camera;

    public BlockDragController(TileManager tileManager, MergePlatformsManager mergePlatformsManager, BlockManager blockManager, Camera camera)
    {
        this.tileManager = tileManager;
        this.mergePlatformsManager = mergePlatformsManager;
        this.blockManager = blockManager;
        this.camera = camera;
    }

    public void StartDragging(BlockParent blockToDrag)
    {
        blockParent = blockToDrag;
        draggingFirstBlock = blockToDrag.gameObject.GetComponentInChildren<Block>();
        dragOffset.y = blockYOffset;

        mergePlatformsManager.SetPlatformFree(blockParent.platform);
        blockParent.StartDragging();
    }

    private readonly Plane horizontalPlane = new Plane(Vector3.up, Vector3.up);

    public void UpdateDragging()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (horizontalPlane.Raycast(ray, out float distance))
            blockParent.SetPosition(ray.GetPoint(distance) + dragOffset);

        TryToHighlight();
    }

    public void TryToHighlight()
    {
        StopHighlight();

        List<Tile> availableTiles = tileManager.CheckAvailableTiles(draggingFirstBlock, blockParent);

        if (availableTiles != null && availableTiles.Count > 0)
        {
            tileManager.HighLightTiles(availableTiles, true);
        }
    }

    public void StopHighlight()
    {
        tileManager.HighLightTiles(tileManager.GetAllTiles().Cast<Tile>().ToList(), false);
    }

    public bool TryMerge(RaycastHit hit, out BlockParent block1, out BlockParent block2)
    {
        block1 = blockParent;
        block2 = null;
        var otherParent = hit.transform.parent?.GetComponent<BlockParent>();

        if (otherParent == null || otherParent == blockParent)
        {
            Debug.Log("No valid block to merge with or trying to merge with itself.");
            return false;
        }

        block2 = otherParent;

        return otherParent.rank == blockParent.rank && !otherParent.isOnBoard;
    }

    public bool TryPlaceOnBoard()
    {
        if (tileManager.CheckAvailableTiles(draggingFirstBlock, blockParent) != null && tileManager.CheckAvailableTiles(draggingFirstBlock, blockParent).Count > 0)
            return true;

        return false;
    }

    public bool TryPlaceOnPlatform(out int platformIndex)
    {
        platformIndex = -1;

        Ray blockRay = new Ray(blockParent.transform.position, Vector3.down);

        if (Physics.Raycast(blockRay, out RaycastHit hit, 5f))
        {
            GameObject hitGO = hit.transform.gameObject;

            if (hitGO.layer == LayerMask.NameToLayer("Platform"))
            {
                Platform platform = hitGO.GetComponent<Platform>();

                if (platform != null && platform.isFree)
                {
                    platformIndex = platform.index;
                    return true;
                }
            }
        }

        return false;
    }

    public void PlaceOnBoard()
    {
        Debug.Log("PlaceOnBoard!");

        List<Tile> availableTiles = tileManager.CheckAvailableTiles(draggingFirstBlock, blockParent)?.ToList();

        Vector3 lastTilePosition = availableTiles.Last().transform.position;
        Vector3 finalPosition = new Vector3((availableTiles.First().transform.position.x + lastTilePosition.x) / 2, 0f, (availableTiles.First().transform.position.z + lastTilePosition.z) / 2);

        blockParent.PlaceOnBoard(availableTiles, finalPosition);

        blockParent = null;
        draggingFirstBlock = null;
    }

    public void PlaceOnPlatform(int platformIndex)
    {
        Debug.Log("PlaceOnPlatform!");

        mergePlatformsManager.SetPlatformBusy(platformIndex);
        blockParent.PlaceOnPlatform(mergePlatformsManager.GetPlatformPositionByIndex(platformIndex), platformIndex);

        blockParent = null;
        draggingFirstBlock = null;
    }

    public void Cancel()
    {
        blockParent.ReturnToPreviousState();

        if (blockParent.IsWasOnPlatform())
            mergePlatformsManager.SetPlatformBusy(blockParent.platform);

        blockParent = null;
        draggingFirstBlock = null;
    }
}
