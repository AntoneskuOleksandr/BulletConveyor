using UnityEngine;

public class DraggingState : IDragState
{
    private readonly DragStateMachine stateMachine;
    private readonly BlockDragController dragController;

    public DraggingState(DragStateMachine stateMachine, BlockDragController dragController)
    {
        this.stateMachine = stateMachine;
        this.dragController = dragController;
    }

    public void Enter() => dragController.TryToHighlight();
    public void Exit() => dragController.StopHighlight();
    public void Update() => dragController.UpdateDragging();

    public void OnMouseDown(RaycastHit hit) { }

    public void OnMouseUp(RaycastHit hit)
    {
        if (dragController.TryMerge(hit))
            dragController.Merge();
        else if (dragController.TryPlaceOnBoard())
            dragController.PlaceOnBoard();
        else if (dragController.TryPlaceOnPlatform())
            dragController.PlaceOnPlatform();
        else
            dragController.Cancel();

        stateMachine.ChangeState(new IdleState(stateMachine, dragController));
    }
}
