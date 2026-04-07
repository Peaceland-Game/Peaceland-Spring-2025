using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class LetterPuzzleMinigame : MinigameBehavior
{
    /// <summary>
    /// Reference to the GameObject used for the puzzle minigame.
    /// </summary>
    [SerializeField] GameObject puzzleMinigame;

    /// <summary>
    /// Reference to the Puzzle Container GameObject
    /// </summary>
    [SerializeField] GameObject puzzleContainer;

    /// <summary>
    /// Prefab used to instantiate piece GameObjects in the scene.
    /// </summary>
    [SerializeField] GameObject piecePrefab;

    /// <summary>
    /// Prefab to instantiate as the target object.
    /// </summary>
    [SerializeField] GameObject targetPrefab;


    /// <summary>
    /// The positions of the pieces when the minigame starts
    /// </summary>
    [SerializeField] Vector3[] pieceLocations;

    /// <summary>
    /// The locations of the targets
    /// </summary>
    [SerializeField] Vector3[] targetLocations;

    /// <summary>
    /// The rotations of the targets
    /// </summary>
    [SerializeField] Vector3[] targetRotations;

    /// <summary>
    /// Number of puzzle pieces
    /// </summary>
    [SerializeField] int count;

    /// <summary>
    /// Sprites for each piece (in order)
    /// </summary>
    [SerializeField] Sprite[] sprites;

    [SerializeField] Draggable[] puzzleList;

    //[SerializeField] Button[] buttons;

    private bool isTransitioning;
    private float timer = 1.5f;

    public GameObject puzzlePieceHolder;
    public GameObject ScrollPiecePrefab;

    /// <summary>
    /// Sets the blur on the camera
    /// </summary>
    private PostProcessVolume ppVolume;

    private void Start()
    {        

        puzzleMinigame.GetComponent<DragManager>().OnCompleted += HandleCompleted;
        //registers HandleCompleted as a listener for the OnCompleted event on the DragManager.
        //When DragManager raises OnCompleted, HandleCompleted method will be invoked.
        
    }

    private void Update()
    {
        if (!isTransitioning)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //Debug.Log("Transitioning to next minigame");
                isTransitioning = false;
                puzzleMinigame.GetComponent<DragManager>().Reset();

                // Assuming this is the victory condition, call NextMinigame
                GameManager.Instance.CurrentMemoryManager.NextMinigame();
            }
            
        }

    }

    /// <summary>
    /// Initiates a transition by setting the transitioning flag and resetting the timer.
    /// </summary>
    public void HandleCompleted()
    {
        isTransitioning = true;
        timer = 0.5f; // Reset timer for transition
    }

    public override void StartMinigame()
    {
        gameObject.SetActive(true);
        puzzleContainer.SetActive(true);

    //    ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();

        Vector3[] dragPos = new Vector3[count];
        Vector3[] targetPos = new Vector3[count];
        Vector3[] targetRot = new Vector3[count];
        int[] pieceIds = new int[count]; // used to identify which piece is which
        


        for (int i = 0; i < count; i++)
        {
            dragPos[i] = pieceLocations[i];
            targetPos[i] = targetLocations[i];
            targetRot[i] = targetRotations[i];
            pieceIds[i] = i;
        }

        Draggable[] pieceList = puzzleMinigame.GetComponent<DragManager>().CreateDragToTarget(
            count,
            piecePrefab,
            targetPrefab,
            dragPos,
            targetPos,
            targetRot,
            pieceIds,
            sprites);

        if  (pieceList.Length > 0)
        {
            //puzzleMinigame.GetComponent<DragManager>().CreateDrag(pieceList);

            for (int i = 0; i < pieceList.Length; i++)
            {
                pieceList[i].GetComponent<Draggable>().originParent = puzzleMinigame.GetComponent<DragManager>(); 
                GameObject holder = Instantiate(ScrollPiecePrefab, puzzlePieceHolder.transform);
                pieceList[i].transform.SetParent(holder.transform, true);
                pieceList[i].transform.localPosition = Vector2.zero;
                pieceList[i].setBoundsOffset(83);
                holder.GetComponent<ScrollPiece>().Constructor(pieceList[i]);
            }
        }

        //puzzleMinigame.GetComponent<DragManager>().disableDraggableObjs();
        //puzzleMinigame.GetComponent<DragManager>().setDraggableObjParents("ScrollContent");

        //GameObject[] draggableObjs = puzzleMinigame.GetComponent<DragManager>().getDraggableObjs();
        //// Debug.Log($"found: {GameObject.FindGameObjectWithTag("ScrollContent")}");
        //for (int i = 0; i < count; i++)
        //{
        //    //buttons[i].GetComponent<PieceButtonBehavior>().setPiece(draggableObjs[i]);
        //    //sets the pieces on the buttons to match the pieces in the minigame
        //    draggableObjs[i].transform.SetParent(GameObject.FindGameObjectWithTag("ScrollContent").transform, true);
        //}


        //Set the minigame to active
        puzzleMinigame.SetActive(true);

        //Adds the blur to minigames with added difficulty
        //if (GameManager.Instance.difficulty > 1)
        //{
        //    ppVolume.enabled = true;
        //    ppVolume.weight = 1;
        //    if (GameManager.Instance.difficulty >= 2)
        //    { //Scales from 2 to 11
        //        ppVolume.weight = 0.45f + (GameManager.Instance.difficulty * 0.05f);
        //    }
        //}
    }


    public override void StopMinigame()
    {
        puzzleMinigame.GetComponent<DragManager>().Reset();

        //Remove the blur from minigames with added difficulty
    //    ppVolume.enabled = false;

        //deactivate the minigame
        puzzleContainer.SetActive(false);
        puzzleMinigame.SetActive(false);
    }
}
