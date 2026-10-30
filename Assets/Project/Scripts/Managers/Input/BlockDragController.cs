using System;
using System.Collections.Generic;
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
    private readonly BoardManager boardManager;
    private readonly Camera camera;

    private readonly float blockDragingYOffset = 0.925f;

    public static event Action<DragResult> OnDragFinished;

    public BlockDragController(TileManager tileManager, BoardManager boardManager, MergePlatformsManager mergePlatformsManager, Camera camera)
    {
        this.tileManager = tileManager;
        this.boardManager = boardManager;
        this.mergePlatformsManager = mergePlatformsManager;
        this.camera = camera;
    }

    public void StartDragging(BlockParent blockToDrag)
    {
        session = new DragSession(blockToDrag, blockDragingYOffset);

        if (blockToDrag.blockData.platform != -1)
            mergePlatformsManager.SetPlatformFree(session.DraggedBlock.blockData.platform);

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

        List<Tile> availableTiles = CheckAvailableTilesForShape(session.DraggedBlock);
        if (availableTiles != null && availableTiles.Count > 0)
            tileManager.HighLightTiles(availableTiles, true);
    }

    public void StopHighlight()
    {
        tileManager.HighLightTiles(tileManager.GetAllTilesList(), false);
    }

    public DragResult FinishDragging(RaycastHit hit)
    {
        if (TryMerge(hit, out var block1, out var block2))
            return new DragResult(DragActionType.Merge, block1, block2);

        if (TryPlaceOnBoard())
            return new DragResult(DragActionType.PlaceOnBoard, session.DraggedBlock);

        if (TryPlaceOnPlatform(out var platformIndex))
            return new DragResult(DragActionType.PlaceOnPlatform, session.DraggedBlock, null, platformIndex);

        return new DragResult(DragActionType.Cancel);
    }

    private bool TryMerge(RaycastHit hit, out BlockParent block1, out BlockParent block2)
    {
        block1 = session.DraggedBlock;
        block2 = hit.transform != null ? hit.transform.GetComponentInParent<BlockParent>() : null;

        if (block2 == null || block1 == block2)
            return false;

        return block2.rank == block1.rank && block1.nextRangBlock != null && !block2.blockData.isOnBoard;
    }

    private bool TryPlaceOnBoard()
    {
        var tiles = CheckAvailableTilesForShape(session.DraggedBlock);
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

        OnDragFinished?.Invoke(result);
        session = null;
    }

    private void PlaceOnBoard(Vector2Int? savedTopLeft = null)
    {
        Vector2Int topLeft = savedTopLeft ?? GetTopLeftIndexByBlockPosition(session.DraggedBlock);
        List<Tile> tiles = CheckAvailableTilesForShape(session.DraggedBlock, topLeft);
        if (tiles == null) return;

        Vector2 pivot = GetMatrixPivot(session.DraggedBlock);
        Vector3 topLeftWorld = tileManager.TileToWorld(topLeft);
        Vector3 finalPos = topLeftWorld + new Vector3(pivot.x * tileManager.CellSize, 0f, pivot.y * tileManager.CellSize);

        session.DraggedBlock.PlaceOnBoard(tiles, finalPos);
        session.DraggedBlock.blockData.topLeftIndex = topLeft;
        boardManager.RegisterBlock(session.DraggedBlock);
        GameAudio.PlayPlace();
    }

    private void PlaceOnPlatform(int platformIndex)
    {
        mergePlatformsManager.SetPlatformBusy(platformIndex);
        session.DraggedBlock.PlaceOnPlatform(mergePlatformsManager.GetPlatformPositionByIndex(platformIndex), platformIndex);
    }

    private void Cancel()
    {
        switch (session.DraggedBlock.blockPreviousState)
        {
            case BlockState.OnPlatform:
                {
                    Debug.Log("Cancel to platform " + session.DraggedBlock.blockPreviousData.platform);
                    PlaceOnPlatform(session.DraggedBlock.blockPreviousData.platform);
                    break;
                }
            case BlockState.OnBoard:
                {
                    Debug.Log("Cancel to board");
                    PlaceOnBoard(session.DraggedBlock.blockData.topLeftIndex);
                    break;
                }
        }
    }

    private Vector2 GetMatrixPivot(BlockParent block)
    {
        int width = block.shape[0].row.Length;
        int height = block.shape.Count;
        float px = (width - 1) / 2f;
        float py = (height - 1) / 2f;
        return new Vector2(px, py);
    }

    private Vector2Int GetTopLeftIndexByBlockPosition(BlockParent block)
    {
        Vector2 tileFloat = tileManager.WorldToTileFloat(block.transform.position);
        Vector2 pivot = GetMatrixPivot(block);
        Vector2 topLeftFloat = tileFloat - pivot;

        return new Vector2Int(Mathf.RoundToInt(topLeftFloat.x), Mathf.RoundToInt(topLeftFloat.y));
    }

    private List<Tile> CheckAvailableTilesForShape(BlockParent block, Vector2Int topLeftIndex)
    {
        List<Tile> result = new List<Tile>();
        int height = block.shape.Count;
        int width = block.shape[0].row.Length;

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (block.shape[r].row[c] == 1)
                {
                    Vector2Int tileIndex = new Vector2Int(topLeftIndex.x + c, topLeftIndex.y + r);

                    if (!tileManager.IsInsideBoard(tileIndex))
                        return null;

                    Tile t = tileManager.GetTileAt(tileIndex);
                    if (t == null || !t.isAvailable)
                        return null;

                    result.Add(t);
                }
            }
        }

        return result;
    }

    private List<Tile> CheckAvailableTilesForShape(BlockParent block)
    {
        Vector2Int topLeft = GetTopLeftIndexByBlockPosition(block);
        return CheckAvailableTilesForShape(block, topLeft);
    }
}
