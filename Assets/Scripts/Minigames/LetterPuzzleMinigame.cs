using Unity.VisualScripting;
//using UnityEditorInternal;
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
    //[SerializeField] GameObject puzzleContainer;

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

    [SerializeField] string[] sentenceTexts;
    [SerializeField] string[] targetHintTexts;
    [SerializeField] int[] pieceMemoryIds;
    [SerializeField] int[] targetMemoryIds;

    //[SerializeField] Button[] buttons;

    private bool isTransitioning;
    private float timer = 1.5f;

    /// <summary>
    /// For scroll view. Uncomment to use. This is the parent object that holds the pieces in the scroll view, and the prefab for the pieces in the scroll view. The piece prefab should be a child of the ScrollPiece prefab, which has the script for dragging from the scroll view to the puzzle area.
    ///</summary>
    //public GameObject puzzlePieceHolder;

    /// <summary>
    /// For scroll view. Uncomment to use. This is the prefab for the pieces in the scroll view. The piece prefab should be a child of the ScrollPiece prefab, which has the script for dragging from the scroll view to the puzzle area.
    ///</summary>
    //public GameObject ScrollPiecePrefab;

    /// <summary>
    /// Sets the blur on the camera
    /// </summary>
    private PostProcessVolume ppVolume;

    // Tap action for ending the minigame. Should work for both PC and Android
    private InputAction tap;

    // The "Tap to Continue" text that pops up after completing the minigame
    /*[SerializeField]
    private GameObject continueText;*/
    [SerializeField]
    private Button continueButton;

    // The box game object that contains a collider to cover the screen
    [SerializeField]
    private GameObject box1;

    [SerializeField]
    private bool autoStart = true;
    private bool isStarted = false;

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.interactable = false;  // Ensure button is disabled
            continueButton.gameObject.SetActive(false);
        }
        if (puzzleMinigame != null && puzzleMinigame.GetComponent<DragManager>() != null)
        {
            puzzleMinigame.GetComponent<DragManager>().OnCompleted += HandleCompleted;
        }

        tap = InputSystem.actions != null ? InputSystem.actions.FindAction("Tap") : null;

        if (autoStart)
        {
            StartMinigame();
        }
    }

    private void Update()
    {
        if (!isTransitioning)
        {
            return;
        }

        if (timer >= 0) timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
                continueButton.interactable = true;
                if (continueButton.GetComponent<Image>() != null)
                    continueButton.GetComponent<Image>().enabled = true;
            }
            if (box1 != null)
            {
                box1.SetActive(false);
            }
        }

    }

    public void Continue()
    {
        isTransitioning = false;
        isStarted = false;
        if (puzzleMinigame != null && puzzleMinigame.GetComponent<DragManager>() != null)
        {
            puzzleMinigame.GetComponent<DragManager>().Reset();
        }

        // Turn off button
        Debug.Log("Continue Button Clicked");
        if (continueButton != null)
        {
            continueButton.interactable = false;
            continueButton.gameObject.SetActive(false);
        }

        // Assuming this is the victory condition, call NextMinigame
        Debug.Log("Next minigame called");
        if (GameManager.Instance != null && GameManager.Instance.CurrentMemoryManager != null)
        {
            GameManager.Instance.CurrentMemoryManager.NextMinigame();
        }
    }

    /// <summary>
    /// Initiates a transition by setting the transitioning flag and resetting the timer.
    /// </summary>
    public void HandleCompleted()
    {
        isTransitioning = true;
        timer = 0.5f; // Reset timer for transition
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            if (continueButton.GetComponent<Image>() != null)
                continueButton.GetComponent<Image>().enabled = true; // Show the continue button
            continueButton.interactable = true;   // Enable text when puzzle is complete
        }
    }

    public override void StartMinigame()
    {
        if (isStarted) return;
        isStarted = true;
        gameObject.SetActive(true);
        //puzzleContainer.SetActive(true);

    //    ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();

        Vector3[] dragPos = new Vector3[count];
        Vector3[] targetPos = new Vector3[count];
        Vector3[] targetRot = new Vector3[count];
        int[] pieceIds = new int[count]; // used to identify which piece is which
        


        bool randomize = (pieceLocations != null && pieceLocations.Length > count);
        System.Collections.Generic.List<int> availableIndices = new System.Collections.Generic.List<int>();
        if (pieceLocations != null)
        {
            for (int j = 0; j < pieceLocations.Length; j++) availableIndices.Add(j);
        }

        for (int i = 0; i < count; i++)
        {
            int locIndex = i;
            if (randomize)
            {
                if (availableIndices.Count > 0)
                {
                    int r = Random.Range(0, availableIndices.Count);
                    locIndex = availableIndices[r];
                    availableIndices.RemoveAt(r);
                }
                else if (pieceLocations.Length > 0)
                {
                    locIndex = Random.Range(0, pieceLocations.Length);
                }
            }

            dragPos[i] = (pieceLocations != null && locIndex < pieceLocations.Length) ? pieceLocations[locIndex] : Vector3.zero;
            targetPos[i] = (targetLocations != null && targetLocations.Length > 0) ? targetLocations[i % targetLocations.Length] : Vector3.zero;
            targetRot[i] = (targetRotations != null && targetRotations.Length > 0) ? targetRotations[i % targetRotations.Length] : Vector3.zero;
            pieceIds[i] = i;
        }

        Draggable[] pieceList;
        DragManager dm = puzzleMinigame.GetComponent<DragManager>();

        if (pieceMemoryIds != null && pieceMemoryIds.Length == count && targetLocations != null && targetLocations.Length > 0)
        {
            int numTargets = targetLocations.Length;
            int[] tTypes = (targetMemoryIds != null && targetMemoryIds.Length == numTargets) ? targetMemoryIds : new int[numTargets];
            if (targetMemoryIds == null || targetMemoryIds.Length != numTargets)
            {
                for (int t = 0; t < numTargets; t++) tTypes[t] = t + 1;
            }

            pieceList = dm.CreateDragWithCustomTargets(
                count,
                numTargets,
                piecePrefab,
                targetPrefab,
                dragPos,
                targetLocations,
                targetRotations,
                pieceMemoryIds,
                tTypes,
                sprites,
                null);
        }
        else
        {
            pieceList = dm.CreateDragToTarget(
                count,
                piecePrefab,
                targetPrefab,
                dragPos,
                targetPos,
                targetRot,
                pieceIds,
                sprites);
        }

        if (sentenceTexts != null && sentenceTexts.Length > 0 && pieceList != null)
        {
            for (int i = 0; i < pieceList.Length; i++)
            {
                if (i < sentenceTexts.Length && pieceList[i] != null)
                {
                    var tmp = pieceList[i].GetComponentInChildren<TMPro.TextMeshPro>();
                    if (tmp != null) tmp.text = sentenceTexts[i];
                }
            }
        }

        if (targetHintTexts != null && targetHintTexts.Length > 0 && puzzleMinigame != null)
        {
            var dmTargets = puzzleMinigame.GetComponentsInChildren<DragTarget>();
            for (int i = 0; i < dmTargets.Length; i++)
            {
                if (i < targetHintTexts.Length && dmTargets[i] != null)
                {
                    var tmp = dmTargets[i].GetComponentInChildren<TMPro.TextMeshPro>();
                    if (tmp != null) tmp.text = targetHintTexts[i];
                }
            }
        }

        if  (pieceList.Length > 0)
        {

            // for scroll view. uncomment to use

            //for (int i = 0; i < pieceList.Length; i++)
            //{
            //    pieceList[i].GetComponent<Draggable>().originParent = puzzleMinigame.GetComponent<DragManager>();
            //    GameObject holder = Instantiate(ScrollPiecePrefab, puzzlePieceHolder.transform);
            //    pieceList[i].transform.SetParent(holder.transform, true);
            //    pieceList[i].transform.localPosition = Vector2.zero;
            //    pieceList[i].setBoundsOffset(83);



            //    holder.GetComponent<ScrollPiece>().Constructor(pieceList[i]);
            //}
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
        //    ppVolume.enabled = false;

        // Unregister the event handler to prevent potential memory leaks or unintended behavior
        puzzleMinigame.GetComponent<DragManager>().OnCompleted -= HandleCompleted;

        //deactivate the minigame

        // If using a puzzle container, deactivate it here. Otherwise, just deactivate the puzzle minigame.
        //puzzleContainer.SetActive(false);
        puzzleMinigame.SetActive(false);
    }
}
