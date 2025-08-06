using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private DragStateMachine stateMachine;
    private Camera cam;

    public void Init(DragStateMachine machine)
    {
        stateMachine = machine;
        cam = Camera.main;
    }

    void Update()
    {
        stateMachine.Update();

        if (Input.GetMouseButtonDown(0))
            Raycast(hit => stateMachine.OnMouseDown(hit));

        if (Input.GetMouseButtonUp(0))
            Raycast(hit => stateMachine.OnMouseUp(hit));
    }

    private void Raycast(System.Action<RaycastHit> callback)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
            callback(hit);
    }
}
