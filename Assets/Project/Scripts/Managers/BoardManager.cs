using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private List<BlockParent> blocks = new List<BlockParent>();
    private Dictionary<BlockParent, UnityAction> listeners = new();

    public void RegisterBlock(BlockParent block)
    {
        if (blocks.Contains(block))
        {
            Debug.LogError("Block already registered in BoardManager: " + block.name);
            return;
        }

        blocks.Add(block);

        UnityAction action = () => { UnregisterBlock(block); };
        listeners[block] = action;
        block.onDragFromBoard.AddListener(action);
    }

    public void UnregisterBlock(BlockParent block)
    {
        Debug.Log("UnregisterBlock " + block.name);
        if (blocks.Contains(block))
        {
            if (listeners.TryGetValue(block, out var action))
            {
                block.onDragFromBoard.RemoveListener(action);
                listeners.Remove(block);
            }
            blocks.Remove(block);
        }
        else
        {
            Debug.LogWarning("Block not found in BoardManager: " + block.name);
        }
    }

}
