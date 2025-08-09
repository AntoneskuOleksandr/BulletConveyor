using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DragActionType
{
    Merge,
    PlaceOnBoard,
    PlaceOnPlatform,
    Cancel
}

public struct DragResult
{
    public DragActionType Action;
    public BlockParent Block1;
    public BlockParent Block2;
    public int PlatformIndex;

    public DragResult(DragActionType action, BlockParent block1 = null, BlockParent block2 = null, int platformIndex = -1)
    {
        Action = action;
        Block1 = block1;
        Block2 = block2;
        PlatformIndex = platformIndex;
    }
}

public class BlockDragController
{
    private DragSession session;
    private readonly TileManager tileManager;
    private readonly MergePlatformsManager mergePlatformsManager;
    private readonly Camera camera;

    private readonly float blockYOffset = 0.925f;

    public BlockDragController(TileManager tileManager, MergePlatformsManager mergePlatformsManager, Camera camera)
    {
        this.tileManager = tileManager;
        this.mergePlatformsManager = mergePlatformsManager;
        this.camera = camera;
    }

    public void StartDragging(BlockParent blockToDrag)
    {
        session = new DragSession(blockToDrag, blockYOffset);

        mergePlatformsManager.SetPlatformFree(session.DraggedBlock.platform);
        session.DraggedBlock.StartDragging();
    }

    private readonly Plane horizontalPlane = new Plane(Vector3.up, Vector3.up);

    public void UpdateDragging()
    {
        if (session == null) return;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (horizontalPlane.Raycast(ray, out float distance))
            session.DraggedBlock.SetPosition(ray.GetPoint(distance) + session.DragOffset);

        TryToHighlight();
    }

    public void TryToHighlight()
    {
        StopHighlight();

        List<Tile> availableTiles = tileManager.CheckAvailableTiles(session.DraggingFirstBlock, session.DraggedBlock);
        if (availableTiles != null && availableTiles.Count > 0)
            tileManager.HighLightTiles(availableTiles, true);
    }

    public void StopHighlight()
    {
        tileManager.HighLightTiles(tileManager.GetAllTiles().Cast<Tile>().ToList(), false);
    }

    public DragResult FinishDragging(RaycastHit hit)
    {
        if (TryMerge(hit, out var block1, out var block2))
            return new DragResult(DragActionType.Merge, block1, block2);

        if (TryPlaceOnBoard())
            return new DragResult(DragActionType.PlaceOnBoard, session.DraggedBlock);

        if (TryPlaceOnPlatform(out var platformIndex))
            return new DragResult(DragActionType.PlaceOnPlatform, session.DraggedBlock, null, platformIndex);

        return new DragResult(DragActionType.Cancel, session.DraggedBlock);
    }

    private bool TryMerge(RaycastHit hit, out BlockParent block1, out BlockParent block2)
    {
        block1 = session.DraggedBlock;
        block2 = hit.transform.parent?.GetComponent<BlockParent>();

        if (block2 == null || block1 == block2)
            return false;

        return block2.rank == block1.rank && !block2.isOnBoard;
    }

    private bool TryPlaceOnBoard()
    {
        var tiles = tileManager.CheckAvailableTiles(session.DraggingFirstBlock, session.DraggedBlock);
        return tiles != null && tiles.Count > 0;
    }

    private bool TryPlaceOnPlatform(out int platformIndex)
    {
        platformIndex = -1;

        Ray blockRay = new Ray(session.DraggedBlock.transform.position, Vector3.down);
        if (Physics.Raycast(blockRay, out RaycastHit hit, 5f))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Platform"))
            {
                var platform = hit.transform.GetComponent<Platform>();
                if (platform != null && platform.isFree)
                {
                    platformIndex = platform.index;
                    return true;
                }
            }
        }
        return false;
    }

    public void ExecuteAction(DragResult result, BlockManager blockManager)
    {
        switch (result.Action)
        {
            case DragActionType.Merge:
                blockManager.MergeBlocks(result.Block1, result.Block2);
                break;
            case DragActionType.PlaceOnBoard:
                PlaceOnBoard();
                break;
            case DragActionType.PlaceOnPlatform:
                PlaceOnPlatform(result.PlatformIndex);
                break;
            case DragActionType.Cancel:
                Cancel();
                break;
        }
        session = null;
    }

    private void PlaceOnBoard()
    {
        var availableTiles = tileManager.CheckAvailableTiles(session.DraggingFirstBlock, session.DraggedBlock)?.ToList();
        Vector3 lastTilePosition = availableTiles.Last().transform.position;
        Vector3 finalPosition = (availableTiles.First().transform.position + lastTilePosition) / 2;
        session.DraggedBlock.PlaceOnBoard(availableTiles, finalPosition);
    }

    private void PlaceOnPlatform(int platformIndex)
    {
        mergePlatformsManager.SetPlatformBusy(platformIndex);
        session.DraggedBlock.PlaceOnPlatform(mergePlatformsManager.GetPlatformPositionByIndex(platformIndex), platformIndex);
    }

    private void Cancel()
    {
        session.DraggedBlock.ReturnToPreviousState();
        if (session.DraggedBlock.IsWasOnPlatform())
            mergePlatformsManager.SetPlatformBusy(session.DraggedBlock.platform);
    }
}
