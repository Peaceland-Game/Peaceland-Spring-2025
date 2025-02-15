using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System;

// Manages Draggable Objects
public class DragManager : MonoBehaviour
{
    [SerializeField]
    private Draggable[] draggables;

    void Update()
    {
        TouchControl touch = Touchscreen.current.primaryTouch;

        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began) {
            Vector3 touch_wp = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());

            int highestOrderInLayer = int.MinValue;
            Draggable candidate = null;
            foreach (var draggable in draggables) {
                // Select the draggable in front
                if (draggable.CanDrag(touch_wp) && draggable.GetComponent<SpriteRenderer>().sortingOrder > highestOrderInLayer) {
                    candidate = draggable;
                }
            }
            if (candidate is not null)
                candidate.StartDrag(touch_wp);
        }
    }

    public static Tuple<TouchControl, Vector3> GetTouchWorldPosition() {
        TouchControl touch = Touchscreen.current.primaryTouch;
        Vector3 touch_wp = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());

        return new Tuple<TouchControl, Vector3>(touch, touch_wp);
    }
}
