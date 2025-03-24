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

    /// <summary>
    /// The starting positions of the flowers to reset them to each time this is restarted
    /// </summary>
    private Vector3[] startingLocations;

    private void InitializeStartingLocations()
    {
        //Initialize the startingLocations array
        startingLocations = new Vector3[draggables.Length];

        //Set the starting location of each flower to save when restarting the arranging minigame
        for (int i = 0; i < draggables.Length; i++)
        {
            startingLocations[i] = draggables[i].gameObject.transform.localPosition;
        }
    }
    
    public override void StartMinigame()
    {
        //Reset the current draggable
        currentDraggable = null;

        //If the startingLocations array has not been initialized and filled, then do it now
        if (startingLocations == null || startingLocations.Length == 0)
        {
            InitializeStartingLocations();
        }

        //For each flower, reset its position to its starting location and make sure they can be dragged
        for (int i = 0; i < draggables.Length; i++)
        {
            draggables[i].gameObject.transform.localPosition = startingLocations[i];
            draggables[i].EnableDrag();
        }

        //Set the arranging minigame to active
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
                //End the drag of the current draggable
                currentDraggable.EndDrag();

                //If the number of flowers that have been arranged is less than the length of the draggables array, continue
                if (flowerArrangeNum < draggables.Length && !currentDraggable.IsDraggable())
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
