using UnityEngine;
using Zenject;

public class InputHandler : MonoBehaviour
{
    private DragStateMachine stateMachine;
    private BlockDragController dragController;
    private Camera mainCamera;

    [Inject]
    public void Construct(BlockManager blockManager, TileManager tileManager, MergePlatformsManager mergePlatformsManager, Camera camera)
    {
        this.mainCamera = camera;

        dragController = new BlockDragController(tileManager, mergePlatformsManager, mainCamera);

        stateMachine = new DragStateMachine();
        stateMachine.ChangeState(new IdleState(stateMachine, dragController, mergePlatformsManager, blockManager));
    }

    private void Update()
    {
        stateMachine.Update();

        if (Input.GetMouseButtonDown(0))
            Raycast(hit => stateMachine.OnMouseDown(hit));

        if (Input.GetMouseButtonUp(0))
            Raycast(hit => stateMachine.OnMouseUp(hit));
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
