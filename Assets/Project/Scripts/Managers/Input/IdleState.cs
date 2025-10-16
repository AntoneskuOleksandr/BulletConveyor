using UnityEngine;

public class IdleState : IDragState
{
    private readonly DragStateMachine stateMachine;
    private readonly BlockDragController dragController;
    private readonly MergePlatformsManager mergePlatformsManager;
    private readonly BlockManager blockManager;

    public IdleState(DragStateMachine stateMachine, BlockDragController dragController, MergePlatformsManager mergePlatformsManager, BlockManager blockManager)
    {
        this.stateMachine = stateMachine;
        this.dragController = dragController;
        this.mergePlatformsManager = mergePlatformsManager;
        this.blockManager = blockManager;
    }

    public void Enter() { }
    public void Exit() { }
    public void Update() { }

    public void OnMouseDown(RaycastHit hit)
    {
        if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Block"))
            return;

        var blockParent = hit.transform.GetComponentInParent<BlockParent>();
        if (blockParent == null)
        {
            Debug.LogError("BlockParent component not found on the hit object.");
            return;
        }

        if (!blockParent.CanDrag)
            return;
        dragController.StartDragging(blockParent);
        stateMachine.ChangeState(new DraggingState(stateMachine, dragController, mergePlatformsManager, blockManager));
    }

    public void OnMouseUp(RaycastHit hit) { }
}
