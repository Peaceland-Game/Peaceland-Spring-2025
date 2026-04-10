using UnityEngine;
using UnityEngine.SceneManagement;

//data that needs to be stored globally and transfered between scenes can be stored and referenced in this script
public class GameManager : MonoBehaviour
{
    //bools for the MuseumIntro scene, used in MuseumIntroManager.cs and SceneTransitioner.cs
    public bool newsRead;
    public bool introSprawlDone;
    public bool marcStart;
    public bool miraIntroDone;
    public bool memoryObjectAcquired;

    // Bools for Demo_RJMuseumIntro
    public bool seenRJMemory = false;

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
    void Start()
    {
        _instance = this;
        DontDestroyOnLoad(_instance);

        cursorHotSpot = Vector2.zero;
        Cursor.SetCursor(defaultCursor, cursorHotSpot, CursorMode.Auto);

        SceneManager.sceneLoaded += FindCurrentMemoryManager;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FindCurrentMemoryManager(Scene scene, LoadSceneMode mode)
    {
        // Set current Memory Manager
        currentMemoryManager = FindFirstObjectByType<GenericMemManager>();

        // Reset the game when entering the title screen
        //if (scene.buildIndex == 0)
        //{
        //    ResetGameManager();
        //}
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


