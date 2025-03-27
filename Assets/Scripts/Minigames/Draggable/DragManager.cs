using UnityEngine;
using UnityEngine.InputSystem;
using static OrderObject;

// Manages Draggable Objects
public class DragManager : MinigameBehavior
{
    /// <summary>
    /// a list of flower objects that the player can drag
    /// </summary>
    Draggable[] draggables;

    /// <summary>
    /// a list of gameObjects the player must drag flowers to
    /// </summary>
    GameObject[] targets;

    [SerializeField] GameObject flowerPrefab;
    [SerializeField] GameObject targetPrefab;

    Draggable currentDraggable = null;

    /// <summary>
    /// Keeps track of the num of flowers currently arranged
    /// </summary>
    int flowerArrangeNum = 0;

    /// <summary>
    /// The starting positions of the flowers to reset them to each time this is restarted
    /// </summary>
    [SerializeField] Vector3[] startingLocations;
    
    public override void StartMinigame()
    {
        //initilize draggables and targets with the number of flowers in the order
        int numberOfFlowers = FlowerShopManager.GetCurrentOrder().flowers.Count;
        draggables = new Draggable[numberOfFlowers];
        targets = new GameObject[numberOfFlowers];
        //instantiate flowers and targets to fill the arrays
        for (int i = 0; i < numberOfFlowers; i++)
        {
            GameObject newFlower = Instantiate(flowerPrefab);
            draggables[i] = newFlower.GetComponent<Draggable>();
            newFlower.transform.parent = transform;

            GameObject newTarget = Instantiate(targetPrefab);
            targets[i] = newTarget;
            newTarget.transform.parent = transform;
        }

        //For each flower, reset its position to its starting location and make sure they can be dragged
        for (int i = 0; i < numberOfFlowers; i++)
        {
            var currentFlower = FlowerShopManager.GetCurrentOrder().flowers[i];

            draggables[i].gameObject.transform.localPosition = startingLocations[i];
            targets[i].transform.localPosition = startingLocations[i] + new Vector3(-5, 0, 0); //for now, targets move to the left of the flowers. subject to change
            draggables[i].EnableDrag();

            //run the constructor of each of the draggables and targets
            draggables[i].Constructor(targets, FlowerShopManager.GetCurrentOrder().flowers[i].flowerType);
            targets[i].GetComponent<DragTarget>().Constructor(FlowerShopManager.GetCurrentOrder().flowers[i].flowerType);
        }

        //Set the arranging minigame to active
        gameObject.SetActive(true);
    }

    public override void StopMinigame()
    {
        //Reset the current draggable
        currentDraggable = null;

        //delete all flower and target objects
        for (int i = 0; i < FlowerShopManager.GetCurrentOrder().flowers.Count; i++)
        {
            Destroy(draggables[i].gameObject);
            Destroy(targets[i]);
        }

        //deactivate the minigame
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
