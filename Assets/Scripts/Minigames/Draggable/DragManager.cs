using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using static OrderObject;

// Manages Draggable Objects
public class DragManager : MonoBehaviour
{
    /// <summary>
    /// Firing event for when minigame is completed
    /// </summary>
    public event System.Action OnCompleted;


/// <summary>
/// a list of flower objects that the player can drag
/// </summary>
[SerializeField]
    public Draggable[] draggables;

    private int numDraggables;

    /// <summary>
    /// a list of gameObjects the player must drag flowers to
    /// </summary>
    private GameObject[] targets;

    /// <summary>
    /// Keeps track of the current object being dragged
    /// </summary>
    Draggable currentDraggable = null;

    public int completedDragCount = 0;

    /// <summary>
    /// Sets the blur on the camera
    /// </summary>
    private PostProcessVolume ppVolume;

    //public override void StartMinigame()
    //{
    //    ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();

    //    // Initilize draggables and targets with the number of flowers in the order
    //    int numberOfFlowers = FlowerShopManager.GetCurrentOrder().flowers.Count;
    //    draggables = new Draggable[numberOfFlowers];
    //    targets = new GameObject[numberOfFlowers];
    //    // Instantiate flowers and targets to fill the arrays
    //    for (int i = 0; i < numberOfFlowers; i++)
    //    {
    //        GameObject newFlower = Instantiate(flowerPrefab);
    //        draggables[i] = newFlower.GetComponent<Draggable>();
    //        newFlower.transform.parent = transform;

    //        GameObject newTarget = Instantiate(targetPrefab);
    //        targets[i] = newTarget;
    //        newTarget.transform.parent = transform;
    //    }

    //    // For each flower, reset its position to its starting location and make sure they can be dragged
    //    for (int i = 0; i < numberOfFlowers; i++)
    //    {
    //        draggables[i].gameObject.transform.localPosition = flowerLocations[i];
    //        targets[i].transform.localPosition = targetLocations[i];
    //        targets[i].transform.eulerAngles = targetRotations[i];
    //        draggables[i].EnableDrag();

    //        //run the constructor of each of the draggables and targets
    //        draggables[i].Constructor(targets, FlowerShopManager.GetCurrentOrder().flowers[i].flowerType);
    //        targets[i].GetComponent<DragTarget>().Constructor(FlowerShopManager.GetCurrentOrder().flowers[i].flowerType);
    //    }

    //    //Set the arranging minigame to active
    //    gameObject.SetActive(true);

    //    //Adds the blur to minigames with added difficulty
    //    if (GameManager.Instance.difficulty > 1)
    //    {
    //        ppVolume.enabled = true;
    //        ppVolume.weight = 1;
    //        if (GameManager.Instance.difficulty >= 2)
    //        { //Scales from 2 to 11
    //            ppVolume.weight = 0.45f + (GameManager.Instance.difficulty * 0.05f);
    //        }
    //    }
    //}

    //public override void StopMinigame()
    //{
    //    // Reset the current draggable
    //    currentDraggable = null;

    //    // Delete all flower and target objects
    //    for (int i = 0; i < FlowerShopManager.GetCurrentOrder().flowers.Count; i++)
    //    {
    //        Destroy(draggables[i].gameObject);
    //        Destroy(targets[i]);
    //    }

    //    //Remove the blur from minigames with added difficulty
    //    ppVolume.enabled = false;

    //    //deactivate the minigame
    //    gameObject.SetActive(false);
    //}


    /// <summary>
    /// Created draggable and target of that draggable and sets their positions, rotations, and data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_numDraggables">number of draggables</param>
    /// <param name="draggablePrefab"></param>
    /// <param name="targetPrefab"></param>
    /// <param name="dragPositions"></param>
    /// <param name="targetPositions"></param>
    /// <param name="targetRotations"></param>
    /// <param name="draggableData"></param>
    /// <param name="sprites"></param>
    public void CreateDragToTarget<T>(int _numDraggables, GameObject draggablePrefab, GameObject targetPrefab,
        Vector3[] dragPositions, Vector3[] targetPositions, Vector3[] targetRotations, T[] draggableData, Sprite[] sprites)
    {
        numDraggables = _numDraggables;
        draggables = new Draggable[numDraggables];
        targets = new GameObject[numDraggables];
        for (int i = 0; i < numDraggables; i++)
        {
            // Instantiate the draggable and target prefabs to fill the arrays
            // this is for draggables
            GameObject dragObj = Instantiate(draggablePrefab);
            draggables[i] = dragObj.GetComponent<Draggable>();
            dragObj.transform.parent = transform;

            GameObject newTarget = Instantiate(targetPrefab);
            targets[i] = newTarget;
            newTarget.transform.parent = transform;
        }

        for (int i = 0 ; i < numDraggables; i++)
        {
            // Set the positions of the draggables and dragPositions
            draggables[i].gameObject.transform.localPosition = dragPositions[i];
            targets[i].transform.localPosition = targetPositions[i];
            targets[i].transform.eulerAngles = targetRotations[i];
            draggables[i].EnableDrag();

            //run the constructor of each of the draggables and targets
            draggables[i].Constructor(targets, draggableData[i], sprites[i]);
            targets[i].GetComponent<DragTarget>().Constructor(draggableData[i], sprites[i]);
        }

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
                currentDraggable.StartDrag(touch_wp, GameManager.Instance.difficulty);
            }
        }
    }

    /// <summary>
    /// Checks if the number of completed drags is equal to the number of draggables, if so, invokes the OnCompleted event and resets the completed drag count
    /// </summary>
    public void CompletionCheck()
    {
        if (completedDragCount >= draggables.Length)
        {
            //reset the number of completed drags and flower arrange num
            completedDragCount = 0;
            
            OnCompleted?.Invoke();

        }

       
    }

    public void Reset()
    {
        // Reset the current draggable
        currentDraggable = null;

        

        // Delete all flower and target objects
        for (int i = 0; i < numDraggables; i++)
        {
            Destroy(draggables[i].gameObject);
            Destroy(targets[i]);
        }

        
    }
}


