using UnityEngine;

public class DragSession
{
    public BlockParent DraggedBlock { get; }
    public Block DraggingFirstBlock { get; }
    public Vector3 DragOffset { get; }

    public DragSession(BlockParent draggedBlock, float yOffset)
    {
        DraggedBlock = draggedBlock;
        DraggingFirstBlock = draggedBlock.GetComponentInChildren<Block>();
        DragOffset = new Vector3(0, yOffset, 0);
    }
}
