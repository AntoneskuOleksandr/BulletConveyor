using UnityEngine;

public class DraggingState : IDragState
{
    private readonly DragStateMachine stateMachine;
    private readonly BlockDragController dragController;
    private readonly MergePlatformsManager mergePlatformsManager;


    public DraggingState(DragStateMachine stateMachine, BlockDragController dragController, MergePlatformsManager mergePlatformsManager)
    {
        this.stateMachine = stateMachine;
        this.dragController = dragController;
        this.mergePlatformsManager = mergePlatformsManager;
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
        else if (dragController.TryPlaceOnPlatform(out int platformIndex))
            dragController.PlaceOnPlatform(platformIndex);
        else
            dragController.Cancel();

        stateMachine.ChangeState(new IdleState(stateMachine, dragController, mergePlatformsManager));
    }
}
