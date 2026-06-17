using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    //make MenuManager instance
    public static MenuManager Instance { get; private set; }

    //Menu fields and corresponding canvas objects
    [SerializeField] private GameObject blankPrefab;
    [SerializeField] private GameObject defaultPrefab;
    [SerializeField] private GameObject pausedPrefab;
    [SerializeField] private GameObject settingsPrefab;
    [SerializeField] private GameObject saveExitPrefab;
    [SerializeField] private GameObject mainMenuPrefab;
    private GameObject blankCanvas;
    private GameObject defaultCanvas;
    private GameObject pausedCanvas;
    private GameObject settingsCanvas;
    private GameObject saveExitCanvas;
    private GameObject mainMenuCanvas;


    //stack for keeping track of previous menu options
    private Stack<GameObject> menuStack = new Stack<GameObject>();
    private GameObject currentMenu;

    //bool to control starting screen and to handle paused state
    [SerializeField] bool isBlank = false;
    
    public bool CanPause
    {
        get { return canPause; }
        set { canPause = value; }
    }
    public bool IsPaused
    {
        get { return isPaused; }
    }
    bool canPause = false;
    bool isPaused = false;
    bool timeSlowed = false;

    private void Awake()
    {
        //make sure there's an instance of the MenuManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //create the menu screens
        ReInstantiate();
    }

    // Update is called once per frame
    void Update()
    {
        //test if there's an issue with the current menu and reset the stack if there is
        if(currentMenu == null || menuStack.Count == 0)
        {
            ResetMenuStack();
        }
    }

    //resets and re-establishes menu stack
    public void ResetMenuStack()
    {
        //delete all still existing menus
        foreach(var m in menuStack)
        {
            if(m) { Destroy(m); };
        }

        //clear the stack to reset it
        menuStack.Clear();

        //start a new stack by creating new blank and default canvases
        ReInstantiate();
    }
   
    /*INTERNAL MANAGER FUNCTIONS*/

    //creates blank and default canvases
    public void ReInstantiate()
    {
        //instantiate the blank and default canvases only if they don't currently exist
        if (!defaultCanvas)
        {
            defaultCanvas = Instantiate(defaultPrefab);
            defaultCanvas.SetActive(false);
        }
        if(!blankCanvas)
        {
            blankCanvas = Instantiate(blankPrefab);
            blankCanvas.SetActive(false);
        }

        //Get the active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Activate specific scene menus
        if (currentScene.name == "DemoStart")
        {
            mainMenuCanvas = Instantiate(mainMenuPrefab);
            mainMenuCanvas.SetActive(true);
            currentMenu = mainMenuCanvas;
        }
        //otherwise, default to default or blank
        else
        {
            //activate blank menu if blank
            if (isBlank)
            {
                blankCanvas.SetActive(true);
                currentMenu = blankCanvas;
            }
            //set the default menu active otherwise
            else
            {
                defaultCanvas.SetActive(true);
                currentMenu = defaultCanvas;
            }
        }

        menuStack.Push(currentMenu);
    }

    /* GENERIC MENU FUNCTIONS */

    //opens a new menu
    public void OpenMenu(GameObject menuToOpen)
    {
        //deactivate the current menu if it exists
        if (currentMenu)
        {
            currentMenu.SetActive(false);
        }
        //activate the new menu
        menuToOpen.SetActive(true);
        currentMenu = menuToOpen;
        menuStack.Push(menuToOpen);
    }

    //backs out and returns to previous menu
    public void CloseMenu()
    {
        //unpause if closing from paused
        if (isPaused && currentMenu == pausedCanvas)
        {
            UnPauseGame();
        }
        //back out by one menu if we're not at the base layer
        if (menuStack.Count > 1)
        {
            menuStack.Pop().SetActive(false);
            currentMenu = menuStack.Peek();
            currentMenu.SetActive(true);
        }
    }

    //backs out of all menus and returns to base menu
    public void CloseAllMenus()
    {
        //unpause if closing from paused
        if (isPaused)
        {
            UnPauseGame();
        }
        //back out by one menu if we're not at the base layer
        if (menuStack.Count > 1)
        {
            //pop all menus until we're at the base
            for (int i = menuStack.Count; i > 1; i--)
            {
                menuStack.Pop().SetActive(false);
            }
            //proceed as normal
            currentMenu = menuStack.Peek();
            currentMenu.SetActive(true);
        }
    }

    //focuses the first button on a menu
    public void FocusButton(GameObject buttonToFocus)
    {
        EventSystem.current.SetSelectedGameObject(buttonToFocus);
    }

    //switches the scene
    public void SwitchScene(String sceneName)
    {
        //reset time if switching from paused
        if ((isPaused && currentMenu == pausedCanvas)
            || (timeSlowed))
        {
            isPaused = false;
            canPause = true;
            timeSlowed = false;
            Time.timeScale = 1.0f;
        }
        //load the new scene
        SceneManager.LoadScene(sceneName);
    }

    /* SPECIFIC MENU FUNCTIONS */

    //quits the game
    public void QuitGame()
    {
        //close out of the editor if within the unity editor
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
        //quit the application
        Application.Quit();
    }

    //opens save before exiting dialogue
    public void OpenSaveExit()
    {
        //ensure there's a settings canvas
        if (saveExitCanvas == null)
        {
            saveExitCanvas = Instantiate(saveExitPrefab);
            saveExitCanvas.SetActive(false);
        }
        OpenMenu(saveExitCanvas);
    }

    //opens settings
    public void OpenSettings()
    {
        //ensure there's a settings canvas
        if (settingsCanvas == null)
        {
            settingsCanvas = Instantiate(settingsPrefab);
            settingsCanvas.SetActive(false);
        }
        OpenMenu(settingsCanvas);
    }

    //pauses the game and creates the paused screen
    public void PauseGame()
    {
        Time.timeScale = 0.0f;
        isPaused = true;
        GameManager.Instance.gameState = GameManager.GameState.Paused;
        //ensure there's a paused canvas before openeing one
        if (!pausedCanvas)
        {
            pausedCanvas = Instantiate(pausedPrefab);
            pausedCanvas.SetActive(false);
        }
        OpenMenu(pausedCanvas);
    }

    //unpauses the game
    public void UnPauseGame()
    {
        GameManager.Instance.gameState = GameManager.GameState.Active;
        Time.timeScale = 1.0f;
        isPaused = false;
    }

    //loads the next level using a level loader (used on the main menu)
    public void LoadNextLevel()
    {
        GameObject loaderObj = GameObject.Find("LevelLoader");
        //make sure loader exists and has correct components as to not crash if anything is missing
        if (loaderObj) 
        { 
            LevelLoader loader = GameObject.Find("LevelLoader").GetComponent<LevelLoader>();
            if (loader)
            {
                //load the next level using the LevelLoader
                loader.LoadNextLevel();
            }
        }
    

    }
}
