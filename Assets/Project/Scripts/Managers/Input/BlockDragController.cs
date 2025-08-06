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
    }

    public void StartHighlight()
    {
        tileManager.HighLightTiles(tileManager.GetAllTiles().Cast<Tile>().ToList(), true);
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

    public bool TryPlace(RaycastHit _)
    {
        Ray blockRay = new Ray(draggingFirstBlock.transform.position, Vector3.down);

        Debug.DrawRay(blockRay.origin, blockRay.direction * 5, Color.red);

        if (Physics.Raycast(blockRay, out RaycastHit hit, 5f))
        {
            GameObject hitGO = hit.transform.gameObject;

            if (hitGO.layer == LayerMask.NameToLayer("Platform"))
            {
                Platform platform = hitGO.GetComponent<Platform>();
                if (platform != null && platform.isFree)
                {
                    return true;
                }
            }
            else if (hitGO.layer == LayerMask.NameToLayer("Tile"))
            {
                Tile tile = hitGO.GetComponent<Tile>();
                if (tile != null)
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }
    public void Merge()
    {
        Debug.Log("Merge!");
        // «десь логика merge (в идеале Ч вынести в MergeHandler)
    }

    public void Place()
    {
        Debug.Log("Place!");
        // «десь логика размещени€ (в идеале Ч PlaceHandler)
    }

    public void Cancel()
    {
        Debug.Log("Cancel!");
        blockParent.MoveToPreviousPosition();
    }
}
