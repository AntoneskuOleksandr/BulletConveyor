using UnityEngine;

public class IdleState : IDragState
{
    private DragStateMachine stateMachine;
    private BlockDragController dragController;

    public IdleState(DragStateMachine stateMachine, BlockDragController dragController)
    {
        this.stateMachine = stateMachine;
        this.dragController = dragController;
    }

    public void Enter() { }
    public void Exit() { }
    public void Update() { }

    public void OnMouseDown(RaycastHit hit)
    {
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block"))
        {
            dragController.StartDragging(hit.transform.parent.gameObject);
            stateMachine.ChangeState(new DraggingState(stateMachine, dragController));
        }
    }

    public void OnMouseUp(RaycastHit hit) { }
}
