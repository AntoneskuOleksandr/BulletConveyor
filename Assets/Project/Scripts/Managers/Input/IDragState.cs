using UnityEngine;

public interface IDragState
{
    void Enter();
    void Update();
    void Exit();
    void OnMouseDown(RaycastHit hit);
    void OnMouseUp(RaycastHit hit);
}
