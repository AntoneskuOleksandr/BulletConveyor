using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private List<BlockParent> blocks = new List<BlockParent>();

    public void RegisterBlock(BlockParent block)
    {
        if (blocks.Contains(block))
        {
            Debug.LogError("Block already registered in BoardManager: " + block.name);
            return;
        }

        blocks.Add(block);
        block.onDragStart.AddListener(() => { UnregisterBlock(block); });
    }

    public void UnregisterBlock(BlockParent block)
    {
        if (blocks.Contains(block))
            blocks.Remove(block);
        else
            Debug.LogWarning("Block not found in BoardManager: " + block.name);
    }
}
