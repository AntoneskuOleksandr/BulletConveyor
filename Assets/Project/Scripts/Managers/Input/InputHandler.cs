using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Zenject;

public class InputHandler : MonoBehaviour
{
    private DragStateMachine stateMachine;
    private BlockDragController dragController;
    private Camera mainCamera;
    private readonly List<RaycastResult> uiHits = new();

    [Inject]
    public void Construct(BlockManager blockManager, TileManager tileManager, BoardManager boardManager, MergePlatformsManager mergePlatformsManager, Camera camera)
    {
        this.mainCamera = camera;

        dragController = new BlockDragController(tileManager, boardManager, mergePlatformsManager, mainCamera);

        stateMachine = new DragStateMachine();
        stateMachine.ChangeState(new IdleState(stateMachine, dragController, mergePlatformsManager, blockManager));
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;
        stateMachine.Update();

        if (Input.GetMouseButtonDown(0))
        {
            bool overControl = false;
            if (EventSystem.current != null)
            {
                uiHits.Clear();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, uiHits);
                foreach (var hit in uiHits)
                    if (hit.gameObject.GetComponentInParent<Selectable>() != null)
                    {
                        overControl = true;
                        break;
                    }
            }
            if (!overControl) Raycast(hit => stateMachine.OnMouseDown(hit));
        }

        if (Input.GetMouseButtonUp(0))
        {

            int layerMask = ~LayerMask.GetMask("DraggingBlock");
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition),
                out RaycastHit hit, Mathf.Infinity, layerMask);
            stateMachine.OnMouseUp(hit);
        }
    }

    private void Raycast(System.Action<RaycastHit> callback)
    {

        int layerMask = ~LayerMask.GetMask("DraggingBlock");

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            callback(hit);
    }
}
