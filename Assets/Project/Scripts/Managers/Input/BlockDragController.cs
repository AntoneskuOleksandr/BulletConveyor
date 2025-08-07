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
    private Camera camera;

    public BlockDragController(TileManager tileManager, MergePlatformsManager mergePlatformsManager, Camera camera)
    {
        this.tileManager = tileManager;
        this.mergePlatformsManager = mergePlatformsManager;
        this.camera = camera;
    }

    public void StartDragging(BlockParent blockToDrag)
    {
        blockParent = blockToDrag;
        draggingFirstBlock = blockToDrag.gameObject.GetComponentInChildren<Block>();
        dragOffset.y = blockYOffset;

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

        if (tileManager.CheckAvailableTiles(draggingFirstBlock, blockParent) != null && tileManager.CheckAvailableTiles(draggingFirstBlock, blockParent).Count > 0)
        {
            tileManager.HighLightTiles(tileManager.CheckAvailableTiles(draggingFirstBlock, blockParent), true);
        }
    }

    public void StopHighlight()
    {
        tileManager.HighLightTiles(tileManager.GetAllTiles().Cast<Tile>().ToList(), false);
    }

    public bool TryMerge(RaycastHit hit)
    {
        var otherParent = hit.transform.parent?.GetComponent<BlockParent>();
        if (otherParent == null || otherParent == blockParent)
            return false;

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


    public void Merge()
    {
        Debug.Log("Merge!");
        // «десь логика merge (в идеале Ч вынести в MergeHandler)
    }

    public void PlaceOnBoard()
    {
        Debug.Log("PlaceOnBoard!");

        List<Tile> availableTiles = tileManager.CheckAvailableTiles(draggingFirstBlock, blockParent)?.ToList();

        Vector3 lastTilePosition = availableTiles.Last().transform.position;
        Vector3 finalPosition = new Vector3((availableTiles.First().transform.position.x + lastTilePosition.x) / 2, blockYOffset, (availableTiles.First().transform.position.z + lastTilePosition.z) / 2);

        blockParent.PlaceOnBoard(availableTiles, finalPosition);

        blockParent.platform = -1;
        blockParent.isOnBoard = true;

        blockParent = null;
        draggingFirstBlock = null;
    }

    public void PlaceOnPlatform(int platformIndex)
    {
        Debug.Log("PlaceOnPlatform!");

        blockParent.PlaceOnPlatform(mergePlatformsManager.GetPlatformPositionByIndex(platformIndex), platformIndex);

        blockParent = null;
        draggingFirstBlock = null;
    }

    public void Cancel()
    {
        blockParent.ReturnToPreviousState();

        blockParent = null;
        draggingFirstBlock = null;
    }
}
