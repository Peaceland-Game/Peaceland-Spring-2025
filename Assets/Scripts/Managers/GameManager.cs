using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Yarn.Unity;

//data that needs to be stored globally and transfered between scenes can be stored and referenced in this script
public class GameManager : MonoBehaviour
{
    //larger gamestate tracking variables
    public enum GameState
    {
        Paused, Active
    };
    public GameState gameState = GameState.Active;
    //player input to manage input modes
    [SerializeField] PlayerInput playerInput;
    [SerializeField] GameManager prefab;

    //bools for specific game menu checks
    public bool dialogueReplayActive;

    //bools for the MuseumIntro scene, used in MuseumIntroManager.cs and SceneTransitioner.cs
    public bool newsRead;
    public bool introSprawlDone;
    public bool marcStart;
    public bool miraIntroDone;
    public bool memoryObjectAcquired;

    // Bools for Demo_RJMuseumIntro
    public bool seenFloristMemory = false;
    public bool seenRJMemory = false;
    public bool seenChildMemory = false;
    public bool seenVillainMemory = false;
    public bool seenBorisMemory = false;

    [SerializeField]
    public bool placeholderCondition=false; //boolean for when Placeholder is the unlock condition, so we can toggle it on and off for testing

    //cursors
    [SerializeField]
    private Texture2D defaultCursor;

    [SerializeField]
    private Texture2D interactCursor;

    private Vector2 cursorHotSpot;

    // Reference to current Memory Manager
    [SerializeField]
    private GenericMemManager currentMemoryManager;
    public GenericMemManager CurrentMemoryManager { get { return currentMemoryManager; } }

    /// <summary>
    /// Used to add difficulty to the minigame. 0 is normal, 1 is shaky hands, and 2 is blurred vision.
    /// </summary>

    public int difficulty = 0;

    //create the private instance
    private static GameManager _instance;

    //create the public reference
    public static GameManager Instance
    {
        get
        {
            if (_instance is null)
                Debug.LogError("GameManager is NULL");

            return _instance;
        }
    }

    //initialize private instance
    //void Start()
    void Awake()
    {
        /* Important Note:
         * The GameManager is found on an object in the DemoStart Screen.
         * Each time the game loads this scene, a new object is created.
         * This method prevents multiple GMs from gaining "DontDestroyOnLoad,
         *      thus preventing multiple GMs from persisting.
         * Best practice for Singletons should prevent a second GM from being created.
         * However, there was not time to do this in Spring 2026
        */

        //If this is a second GM being created, destroy itself
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Only set the instance if there isn't one already
        if (_instance == null)
        {
            Debug.Log("Setting GM Instance");
            _instance = this;
            DontDestroyOnLoad(_instance);
        
            cursorHotSpot = Vector2.zero;
            Cursor.SetCursor(defaultCursor, cursorHotSpot, CursorMode.Auto);

            SceneManager.sceneLoaded += GMOnSceneLoaded;
        }
        _instance = this;
        Debug.Log("RJ complete status: " + this.seenRJMemory);

       playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Runs all behavior for when a new scene is loaded.
    /// </summary>
    /// <param name="scene">The scene Unity is in.</param>
    /// <param name="mode"> The current LoadSceneMode</param>
    public void GMOnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Set current Memory Manager
        currentMemoryManager = FindFirstObjectByType<GenericMemManager>();

        // Reset the game when re-entering the title screen
        if (scene.buildIndex == 0)
        {
            ResetGameManager();
        }
    }

    //cursor methods for entering/exiting buttons/colliders
    public void OnButtonCursorEnter()
    {
        Cursor.SetCursor(interactCursor, cursorHotSpot, CursorMode.Auto);
    }

    public void OnButtonCursorExit()
    {
        Cursor.SetCursor(defaultCursor, cursorHotSpot, CursorMode.Auto);
    }

    private void EnableActionMapPauseUI()
    {
        playerInput.SwitchCurrentActionMap("PauseUI");
    }

    private void EnablePlayerAction()
    {
        playerInput.SwitchCurrentActionMap("UI");
    }

    private void OnMouseEnter()
    {
        Cursor.SetCursor(interactCursor, cursorHotSpot, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(defaultCursor, cursorHotSpot, CursorMode.Auto);
    }

    /// <summary>
    /// Clears the booleans that control the intro sequence, resetting the demo
    /// </summary>
    private void ResetGameManager()
    {
        Debug.Log("Reseting GameManager bools.");
        newsRead = false;
        introSprawlDone = false;
        marcStart = false;
        miraIntroDone = false;
        memoryObjectAcquired = false;
        seenRJMemory = false;
    }
}


