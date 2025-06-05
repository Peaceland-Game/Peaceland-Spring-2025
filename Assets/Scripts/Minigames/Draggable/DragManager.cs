using UnityEngine;
using UnityEngine.InputSystem;
using static OrderObject;

// Manages Draggable Objects
public class DragManager : MinigameBehavior
{
    /// <summary>
    /// Draggable flower prefab to spawn
    /// </summary>
    [SerializeField] GameObject flowerPrefab;
    
    /// <summary>
    /// Drag target prefab tp spawn
    /// </summary>
    [SerializeField] GameObject targetPrefab;

    /// <summary>
    /// The positions of the flowers when the minigame starts
    /// </summary>
    [SerializeField] Vector3[] flowerLocations;

    /// <summary>
    /// The locations of the targets
    /// </summary>
    [SerializeField] Vector3[] targetLocations;

    /// <summary>
    /// The rotations of the targets
    /// </summary>
    [SerializeField] Vector3[] targetRotations;

    /// <summary>
    /// a list of flower objects that the player can drag
    /// </summary>
    [SerializeField]
    public Draggable[] draggables;

    /// <summary>
    /// a list of gameObjects the player must drag flowers to
    /// </summary>
    private GameObject[] targets;

    /// <summary>
    /// Keeps track of the current object being dragged
    /// </summary>
    Draggable currentDraggable = null;

    /// <summary>
    /// Keeps track of the num of flowers currently arranged
    /// </summary>
    public int flowerArrangeNum = 0;

    public override void StartMinigame()
    {
        // Initilize draggables and targets with the number of flowers in the order
        int numberOfFlowers = FlowerShopManager.GetCurrentOrder().flowers.Count;
        draggables = new Draggable[numberOfFlowers];
        targets = new GameObject[numberOfFlowers];
        // Instantiate flowers and targets to fill the arrays
        for (int i = 0; i < numberOfFlowers; i++)
        {
            GameObject newFlower = Instantiate(flowerPrefab);
            draggables[i] = newFlower.GetComponent<Draggable>();
            newFlower.transform.parent = transform;

            GameObject newTarget = Instantiate(targetPrefab);
            targets[i] = newTarget;
            newTarget.transform.parent = transform;
        }

        // For each flower, reset its position to its starting location and make sure they can be dragged
        for (int i = 0; i < numberOfFlowers; i++)
        {
            draggables[i].gameObject.transform.localPosition = flowerLocations[i];
            targets[i].transform.localPosition = targetLocations[i];
            targets[i].transform.eulerAngles = targetRotations[i];
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
        // Reset the current draggable
        currentDraggable = null;

        // Delete all flower and target objects
        for (int i = 0; i < FlowerShopManager.GetCurrentOrder().flowers.Count; i++)
        {
            Destroy(draggables[i].gameObject);
            Destroy(targets[i]);
        }

        //deactivate the minigame
        gameObject.SetActive(false);
    }

    public void OnTouch(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled) return;

        if (context.phase == InputActionPhase.Disabled || context.phase == InputActionPhase.Canceled)
        {
            if (currentDraggable is not null)
            {
                //End the drag of the current draggable
                currentDraggable.EndDrag();

                //If the number of flowers that have been arranged is less than the length of the draggables array, continue
                if (flowerArrangeNum < draggables.Length && !currentDraggable.IsDraggable())
                {
                    //Increment the num of flowers arranged
                    //flowerArrangeNum++;
                }
            }
        }
        else if (context.phase == InputActionPhase.Started)
        {
            Vector3 touch_wp = InputHelper.GetPointerWorldPosition();
            int highestOrderInLayer = int.MinValue;
            Draggable candidate = null;
            foreach (var draggable in draggables)
            {
                // Select the draggable in front
                if (draggable.CanDrag(touch_wp) && draggable.GetComponent<SpriteRenderer>().sortingOrder > highestOrderInLayer)
                {
                    candidate = draggable;
                }
            }
            if (candidate is not null)
            {
                currentDraggable = candidate;
                currentDraggable.StartDrag(touch_wp);
            }
        }
    }
}
