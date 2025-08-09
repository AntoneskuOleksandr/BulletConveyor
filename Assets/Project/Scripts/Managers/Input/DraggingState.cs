using UnityEngine;

public class DraggingState : IDragState
{
    private readonly DragStateMachine stateMachine;
    private readonly BlockDragController dragController;
    private readonly MergePlatformsManager mergePlatformsManager;
    private readonly BlockManager blockManager;


    public DraggingState(DragStateMachine stateMachine, BlockDragController dragController, MergePlatformsManager mergePlatformsManager, BlockManager blockManager)
    {
        this.stateMachine = stateMachine;
        this.dragController = dragController;
        this.mergePlatformsManager = mergePlatformsManager;
        this.blockManager = blockManager;
    }

    public void Enter() => dragController.TryToHighlight();
    public void Exit() => dragController.StopHighlight();
    public void Update() => dragController.UpdateDragging();

    public void OnMouseDown(RaycastHit hit) { }

    public void OnMouseUp(RaycastHit hit)
    {
        if (dragController.TryMerge(hit, out BlockParent block1, out BlockParent block2))
            blockManager.MergeBlocks(block1, block2);
        else if (dragController.TryPlaceOnBoard())
            dragController.PlaceOnBoard();
        else if (dragController.TryPlaceOnPlatform(out int platformIndex))
            dragController.PlaceOnPlatform(platformIndex);
        else
            dragController.Cancel();

        stateMachine.ChangeState(new IdleState(stateMachine, dragController, mergePlatformsManager, blockManager));
    }
}
