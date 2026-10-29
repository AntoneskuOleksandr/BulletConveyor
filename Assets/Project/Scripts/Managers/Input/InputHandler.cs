using UnityEngine;
using Zenject;

public class InputHandler : MonoBehaviour
{
    private DragStateMachine stateMachine;
    private BlockDragController dragController;
    private Camera mainCamera;

    [Inject]
    public void Construct(BlockManager blockManager, TileManager tileManager, BoardManager boardManager, MergePlatformsManager mergePlatformsManager, Camera camera)
    {
        this.mainCamera = camera;

        dragController = new BlockDragController(tileManager, boardManager, mergePlatformsManager, mainCamera);

        stateMachine = new DragStateMachine();
        stateMachine.ChangeState(new IdleState(stateMachine, dragController, mergePlatformsManager, blockManager));
    }

    private void Update()
    {
        stateMachine.Update();

        if (Input.GetMouseButtonDown(0))
            Raycast(hit => stateMachine.OnMouseDown(hit));

        if (Input.GetMouseButtonUp(0))
        {
            // A release outside all colliders must still finish/cancel the drag.
            int layerMask = ~LayerMask.GetMask("DraggingBlock");
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition),
                out RaycastHit hit, Mathf.Infinity, layerMask);
            stateMachine.OnMouseUp(hit);
        }
    }

    private void Raycast(System.Action<RaycastHit> callback)
    {
        // Игнорируем слой "DraggingBlock"
        int layerMask = ~LayerMask.GetMask("DraggingBlock");

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            callback(hit);
    }
}
