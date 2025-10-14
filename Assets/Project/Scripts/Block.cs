using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector2Int positionIndex;

    private void Start()
    {
        positionIndex = -Vector2Int.one;
    }
}
