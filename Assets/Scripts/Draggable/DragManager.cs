using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System;

// Manages Draggable Objects
public class DragManager : MonoBehaviour
{
    [SerializeField]
    private Draggable[] draggables;

    private Draggable currentDraggable = null;

    void Update()
    {
    }

    public void OnTouch(InputAction.CallbackContext context) {
        if (!isActiveAndEnabled) return;

        if (context.phase == InputActionPhase.Disabled || context.phase == InputActionPhase.Canceled) {
            if (currentDraggable is not null)
                currentDraggable.EndDrag();
        }
        else if (context.phase == InputActionPhase.Started){
            Vector3 touch_wp = InputHelper.GetPointerWorldPosition();
            int highestOrderInLayer = int.MinValue;
            Draggable candidate = null;
            foreach (var draggable in draggables) {
                // Select the draggable in front
                if (draggable.CanDrag(touch_wp) && draggable.GetComponent<SpriteRenderer>().sortingOrder > highestOrderInLayer) {
                    candidate = draggable;
                }
            }
            if (candidate is not null) {
                currentDraggable = candidate;
                currentDraggable.StartDrag(touch_wp);
            }
        }
    }
}
