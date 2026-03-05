using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using static OrderObject;

// Manages Draggable Objects
public class DragManager : MonoBehaviour
{
    // NOTE: May need to create other functions to make this more "generic" for draggables in general.
    // Draggable.cs will require some changes, potentially making it a parent and creating subclasses of different types of draggables (inventory, other functionality)
    // this includes DragTarget.cs as well.


    /// <summary>
    /// Firing event for when minigame is completed
    /// </summary>
    public event System.Action OnCompleted;


    /// <summary>
    /// a list of draggable objects that the player can drag
    /// </summary>
    [SerializeField]
    public Draggable[] draggables;

    /// <summary>
    /// Keeps track of the number of draggables, used for resetting and completion check
    /// </summary>
    private int numDraggables;

    /// <summary>
    /// a list of gameObjects the player must drag to, the targets
    /// </summary>
    private GameObject[] targets;

    /// <summary>
    /// Keeps track of the current object being dragged
    /// </summary>
    Draggable currentDraggable = null;

    /// <summary>
    /// Stores the number of completed drag operations.
    /// </summary>
    public int completedDragCount = 0;




    /// <summary>
    /// Created draggable and target of that draggable and sets their positions, rotations, and data
    /// *** Can be used generically for any type of draggable and target data, as long as the draggable and target prefabs can handle that data type in their constructors. ***
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

        for (int i = 0; i < numDraggables; i++)
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


    /// <summary>
    /// Handles touch input to start or end dragging of draggable objects based on the input action phase.
    /// Auto calls.
    /// </summary>
    /// <param name="context">The input action callback context containing information about the touch event.</param>
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
            //reset the number of completed drags and successfully drag to target num
            completedDragCount = 0;

            OnCompleted?.Invoke();

        }


    }

    /// <summary>
    /// Resets the current draggable and destroys all draggable and target objects.
    /// </summary>
    public void Reset()
    {
        // Reset the current draggable
        currentDraggable = null;



        // Delete all draggable and target objects
        for (int i = 0; i < numDraggables; i++)
        {
            Destroy(draggables[i].gameObject);
            Destroy(targets[i]);
        }


    }
}


