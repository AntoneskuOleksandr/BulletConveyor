using UnityEngine;

public class DragStateMachine
{
    public IDragState CurrentState { get; private set; }

    public void ChangeState(IDragState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update() => CurrentState?.Update();
    public void OnMouseDown(RaycastHit hit) => CurrentState?.OnMouseDown(hit);
    public void OnMouseUp(RaycastHit hit) => CurrentState?.OnMouseUp(hit);
}
