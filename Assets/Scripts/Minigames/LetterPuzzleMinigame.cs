using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;

public class LetterPuzzleMinigame : MinigameBehavior
{
    /// <summary>
    /// Reference to the GameObject used for the puzzle minigame.
    /// </summary>
    [SerializeField] GameObject puzzleMinigame;

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

    [SerializeField] Button[] buttons;

    private bool isTransitioning;
    private float timer = 0.5f;

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
            //Debug.Log("Transitioning to next minigame");
            isTransitioning = false;
            puzzleMinigame.GetComponent<DragManager>().Reset();
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
        ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();

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

        puzzleMinigame.GetComponent<DragManager>().CreateDragToTarget(
            count,
            piecePrefab,
            targetPrefab,
            dragPos,
            targetPos,
            targetRot,
            pieceIds,
            sprites);

        //puzzleMinigame.GetComponent<DragManager>().disableDraggableObjs();

        GameObject[] draggableObjs = puzzleMinigame.GetComponent<DragManager>().getDraggableObjs();
        for (int i = 0; i < count; i++)
        {
            buttons[i].GetComponent<PieceButtonBehavior>().setPiece(draggableObjs[i]);
            //sets the pieces on the buttons to match the pieces in the minigame
        }




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
        ppVolume.enabled = false;

        //deactivate the minigame
        puzzleMinigame.SetActive(false);
    }
}
