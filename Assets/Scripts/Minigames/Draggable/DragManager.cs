using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using static OrderObject;

// Manages Draggable Objects
public class DragManager : MinigameBehavior
{
    /// <summary>
    /// Draggable object prefab to spawn
    /// </summary>
    [SerializeField] GameObject draggablePrefab;
    
    /// <summary>
    /// Drag target prefab tp spawn
    /// </summary>
    [SerializeField] GameObject targetPrefab;

    /// <summary>
    /// The positions of the draggables when the minigame starts
    /// </summary>
    [SerializeField] Vector3[] draggableLocations;

    /// <summary>
    /// The locations of the drag targets
    /// </summary>
    [SerializeField] Vector3[] targetLocations;

    /// <summary>
    /// The rotations of the drag targets
    /// </summary>
    [SerializeField] Vector3[] targetRotations;

    /// <summary>
    /// Used to add difficulty to the minigame. 0 is normal, 1 is shaky hands, and 2 is blurred vision.
    /// </summary>
    [SerializeField] int difficulty;

    /// <summary>
    /// a list of draggable objects that the player can drag
    /// </summary>
    [SerializeField]
    public Draggable[] draggables;

    /// <summary>
    /// a list of gameObjects the player must drag draggables to
    /// </summary>
    private GameObject[] targets;

    /// <summary>
    /// Keeps track of the current object being dragged
    /// </summary>
    Draggable currentDraggable = null;

    /// <summary>
    /// Keeps track of the num of draggables currently arranged
    /// </summary>
    public int draggableArrangeNum = 0;

    /// <summary>
    /// Sets the blur on the camera
    /// </summary>
    private PostProcessVolume ppVolume;

    public override void StartMinigame()
    {
        ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();

        // Initilize draggables and targets with the number of draggables in the order
        int numberOfDraggables = FlowerShopManager.GetCurrentOrder().objects.Count;
        draggables = new Draggable[numberOfDraggables];
        targets = new GameObject[numberOfDraggables];

        // Instantiate draggables and targets to fill the arrays
        for (int i = 0; i < numberOfDraggables; i++)
        {
            GameObject newDraggable = Instantiate(draggablePrefab);
            draggables[i] = newDraggable.GetComponent<Draggable>();
            newDraggable.transform.parent = transform;

            GameObject newTarget = Instantiate(targetPrefab);
            targets[i] = newTarget;
            newTarget.transform.parent = transform;
        }

        // For each draggable, reset its position to its starting location and make sure they can be dragged
        for (int i = 0; i < numberOfDraggables; i++)
        {
            draggables[i].gameObject.transform.localPosition = draggableLocations[i];
            targets[i].transform.localPosition = targetLocations[i];
            targets[i].transform.eulerAngles = targetRotations[i];
            draggables[i].EnableDrag();

            //run the constructor of each of the draggables and targets
            draggables[i].Constructor(targets, FlowerShopManager.GetCurrentOrder().objects[i].objectType);
            targets[i].GetComponent<DragTarget>().Constructor(FlowerShopManager.GetCurrentOrder().objects[i].objectType);
        }

        //Set the arranging minigame to active
        gameObject.SetActive(true);

        //Adds the blur to minigames with added difficulty
        if (difficulty > 1)
        {
            ppVolume.enabled = true;
            ppVolume.weight = 1;
            if (difficulty >= 2)
            {
                ppVolume.weight = 0.6f + (difficulty * 0.05f);
            }
        }
    }

    public override void StopMinigame()
    {
        // Reset the current draggable
        currentDraggable = null;

        // Delete all flower and target objects
        for (int i = 0; i < FlowerShopManager.GetCurrentOrder().objects.Count; i++)
        {
            Destroy(draggables[i].gameObject);
            Destroy(targets[i]);
        }

        //Remove the blur from minigames with added difficulty
        ppVolume.enabled = false;

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
                currentDraggable.StartDrag(touch_wp, difficulty);
            }
        }
    }
}
