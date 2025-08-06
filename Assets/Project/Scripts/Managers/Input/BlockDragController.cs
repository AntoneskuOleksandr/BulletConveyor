using System.Linq;
using UnityEngine;

public class BlockDragController
{
    private GameObject draggingGO;
    private BlockParent blockParent;
    private Block draggingBlock;

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
        Debug.Log("Start Dragging: " + blockGO.name);
        draggingGO = blockGO;
        blockParent = blockGO.GetComponent<BlockParent>();
        draggingBlock = blockGO.GetComponentInChildren<Block>();
        dragOffset.y += blockYOffset; // опционально можно мен€ть

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
        if (otherParent == null)
            return false;

        return otherParent.rank == blockParent.rank && !otherParent.isOnBoard;
    }

    public bool TryPlace(RaycastHit hit)
    {
        return hit.transform.CompareTag("Tile"); // упрощЄнно
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
        draggingGO.transform.position = blockParent.previousPosition;
    }
}
