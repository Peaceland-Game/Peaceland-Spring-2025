using UnityEngine;

//data that needs to be stored globally and transfered between scenes can be stored and referenced in this script
public class GameManager : MonoBehaviour
{
    //bools for the MuseumIntro scene, used in MuseumIntroManager.cs and SceneTransitioner.cs
    public bool newsRead;
    public bool introSprawlDone;
    public bool marcStart;
    public bool miraIntroDone;
    public bool memoryObjectAcquired;

    //cursors
    [SerializeField]
    private Texture2D defaultCursor;

    [SerializeField]
    private Texture2D interactCursor;

    private Vector2 cursorHotSpot;

    /// <summary>
    /// Used to add difficulty to the minigame. 0 is normal, 1 is shaky hands, and 2 is blurred vision.
    /// </summary>
    [HideInInspector]
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
    }

    // Update is called once per frame
    void Update()
    {

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
}


