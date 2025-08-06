using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockDragController
{
    private GameObject draggingGO;
    private BlockParent blockParent;
    private Block draggingFirstBlock;

    private Vector3 dragOffset;
    private float blockYOffset = 0.925f;
    private TileManager tileManager;
    private Camera camera;

    private Tile currentFirstTile;

    public BlockDragController(TileManager tileManager, Camera camera)
    {
        this.tileManager = tileManager;
        this.camera = camera;
    }

    public void StartDragging(GameObject blockGO)
    {
        draggingGO = blockGO;
        blockParent = blockGO.GetComponent<BlockParent>();
        draggingFirstBlock = blockGO.GetComponentInChildren<Block>();
        dragOffset.y = blockYOffset;

        // освободить тайлы
        foreach (var tile in blockParent.myTiles)
            tile.isAvailable = true;
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

        if (CheckAvailableTiles() != null && CheckAvailableTiles().Count > 0)
        {
            tileManager.HighLightTiles(CheckAvailableTiles(), true);
        }
    }

    public void StopHighlight()
    {
        tileManager.HighLightTiles(tileManager.GetAllTiles().Cast<Tile>().ToList(), false);
    }

    private List<Tile> CheckAvailableTiles()
    {
        Ray blockRay = new Ray(draggingFirstBlock.transform.position, Vector3.down);

        Debug.DrawRay(blockRay.origin, blockRay.direction * 5, Color.red);

        if (GetTileUnderFirstBlock() != null)
            return tileManager.GetAvailableTiles(blockParent, GetTileUnderFirstBlock());

        return null;
    }

    public bool TryMerge(RaycastHit hit)
    {
        var otherParent = hit.transform.parent?.GetComponent<BlockParent>();
        if (otherParent == null || otherParent == blockParent)
            return false;

        return otherParent.rank == blockParent.rank && !otherParent.isOnBoard;
    }

    public bool TryPlace()
    {
        if (CheckAvailableTiles() != null && CheckAvailableTiles().Count > 0)
            return true;

        return false;
    }

    public void Merge()
    {
        Debug.Log("Merge!");
        // «десь логика merge (в идеале Ч вынести в MergeHandler)
    }

    private Tile GetTileUnderFirstBlock()
    {
        Ray blockRay = new Ray(draggingFirstBlock.transform.position, Vector3.down);

        if (Physics.Raycast(blockRay, out RaycastHit hit, 5f))
        {
            GameObject hitGO = hit.transform.gameObject;

            if (hitGO.layer == LayerMask.NameToLayer("Tile"))
            {
                Tile tile = hitGO.GetComponent<Tile>();

                if (tile != null)
                {
                    currentFirstTile = tile;
                    return tile;
                }
            }
        }

        return null;
    }

    public void Place()
    {
        Debug.Log("Place!");

        List<Tile> availableTiles = CheckAvailableTiles();

        blockParent.myTiles = availableTiles; // обновить тайлы

        foreach (var tile in blockParent.myTiles)
        {
            tile.isAvailable = false; // зан€ть тайлы
        }

        Vector3 lastTilePosition = availableTiles.Last().transform.position;
        Vector3 finalPosition = new Vector3((availableTiles.First().transform.position.x + lastTilePosition.x) / 2, blockYOffset, (availableTiles.First().transform.position.z + lastTilePosition.z) / 2);

        blockParent.SetPosition(finalPosition);
        blockParent.previousPosition = finalPosition;

        /*        bulletGenerator.blockParents.Add(blockParent);
                blockParent.transform.SetParent(bulletGenerator.blocksPool.transform);

                if (blockParent.platform != -1)
                    platformsManager.platforms[blockParent.platform].isFree = true;

                UIManager.UpdateUI();*/


        blockParent.platform = -1;
        blockParent.isOnBoard = true;

        draggingGO = null;
        blockParent = null;
        draggingFirstBlock = null;
    }

    public void Cancel()
    {
        Debug.Log("Cancel!");
        blockParent.MoveToPreviousPosition();
    }
}
