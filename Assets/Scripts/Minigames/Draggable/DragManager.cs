using UnityEngine;
using UnityEngine.InputSystem;

// Manages Draggable Objects
public class DragManager : MinigameBehavior
{
    [SerializeField]
    private Draggable[] draggables;

    private Draggable currentDraggable = null;

    /// <summary>
    /// Keeps track of the num of flowers currently arranged
    /// </summary>
    private int flowerArrangeNum = 0;

    public override void StartMinigame()
    {
        currentDraggable = null;
        gameObject.SetActive(true);
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }

    public void OnTouch(InputAction.CallbackContext context) {
        if (!isActiveAndEnabled) return;

        if (context.phase == InputActionPhase.Disabled || context.phase == InputActionPhase.Canceled) {
            if (currentDraggable is not null)
            {
                //If the number of flowers that have been arranged is less than the length of the draggables array, continue
                if (flowerArrangeNum < draggables.Length)
                {
                    //Increment the num of flowers arranged
                    flowerArrangeNum++;

                    //If the num of flowers is greater than or equal to the length of the draggables array, stop the minigame and
                    //reset the num of flowers arranged
                    if (flowerArrangeNum >= draggables.Length)
                    {
                        flowerArrangeNum = 0;
                        FlowerShopManager.Instance.NextMinigame();
                    }
                }

                //End the drag of the current draggable
                currentDraggable.EndDrag();
            }
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
