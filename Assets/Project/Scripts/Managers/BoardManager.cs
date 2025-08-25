using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private List<BlockParent> blocks = new List<BlockParent>();
    [SerializeField] private Transform blocksPool;

    private Dictionary<BlockParent, UnityAction> listeners = new();
    private BlockManager _blockManager;

    public List<BlockParent> Blocks
    {
        get { return blocks; }
    }

    [Inject]
    public void Construct(BlockManager blockManager)
    {
        _blockManager = blockManager;
    }

    public void RegisterBlock(BlockParent block)
    {
        if (blocks.Contains(block))
        {
            Debug.LogError("Block already registered in BoardManager: " + block.name);
            return;
        }

        blocks.Add(block);
        block.transform.SetParent(blocksPool);

        UnityAction action = () => { UnregisterBlock(block); };
        listeners[block] = action;
        block.onDragFromBoard.AddListener(action);
    }

    public void UnregisterBlock(BlockParent block)
    {
        if (blocks.Contains(block))
        {
            if (listeners.TryGetValue(block, out var action))
            {
                block.onDragFromBoard.RemoveListener(action);
                listeners.Remove(block);
            }
            blocks.Remove(block);
            block.transform.SetParent(_blockManager.transform);
        }
        else
        {
            Debug.LogWarning("Block not found in BoardManager: " + block.name);
        }
    }
}
