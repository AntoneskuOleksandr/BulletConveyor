using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private MergePlatformsManager mergePlatformsManager;
    [SerializeField] private BlockManager blockManager;
    private DragStateMachine stateMachine;
    private Camera cam;
    private BlockDragController dragController;

    private void Awake()
    {
        cam = Camera.main;
        stateMachine = new DragStateMachine();
        dragController = new BlockDragController(tileManager, mergePlatformsManager, blockManager, cam);
        stateMachine.ChangeState(new IdleState(stateMachine, dragController, mergePlatformsManager, blockManager));
    }

    private void Update()
    {
        stateMachine.Update();

        if (Input.GetMouseButtonDown(0))
            Raycast(hit => stateMachine.OnMouseDown(hit));

        if (Input.GetMouseButtonUp(0))
            Raycast(hit => stateMachine.OnMouseUp(hit));
    }

    private void Raycast(System.Action<RaycastHit> callback)
    {
        int layerMask = ~LayerMask.GetMask("DraggingBlock"); // инвертируем, чтобы ВСЕ слои кроме этого участвовали

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            callback(hit);
    }

}
