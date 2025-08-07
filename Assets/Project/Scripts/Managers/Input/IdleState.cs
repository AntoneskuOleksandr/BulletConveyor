using UnityEngine;

public class IdleState : IDragState
{
    private DragStateMachine stateMachine;
    private BlockDragController dragController;
    private MergePlatformsManager mergePlatformsManager;

    public IdleState(DragStateMachine stateMachine, BlockDragController dragController, MergePlatformsManager mergePlatformsManager)
    {
        this.stateMachine = stateMachine;
        this.dragController = dragController;
        this.mergePlatformsManager = mergePlatformsManager;
    }

    public void Enter() { }
    public void Exit() { }
    public void Update() { }

    public void OnMouseDown(RaycastHit hit)
    {
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block"))
        {
            BlockParent blockParent = hit.transform.GetComponent<BlockParent>();

            if (blockParent == null)
            {
                Debug.LogError("BlockParent component not found on the hit object.");
                return;
            }

            dragController.StartDragging(blockParent);
            mergePlatformsManager.ReleasePlatform(blockParent.platform);
            stateMachine.ChangeState(new DraggingState(stateMachine, dragController, mergePlatformsManager));
        }
    }

    public void OnMouseUp(RaycastHit hit) { }
}
