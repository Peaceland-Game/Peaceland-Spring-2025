using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager script for the War Room; depends on GameManager
/// </summary>

public class F26_Demo_WarRoom : GenericMemManager
{
    private F26_GameManager GM;     // Reference to the Game Manager

    [SerializeField]
    public Button returnToLobbyButton;

    //return to wideshot
    [SerializeField]
    public Button exitWallView;

    //move left one wall
    [SerializeField]
    public Button leftArrow;

    //move right one wall
    [SerializeField]
    public Button rightArrow;

    //the wall buttons
    [SerializeField]
    public Button[] clickableWalls;

    //left wall artifacts 
    [SerializeField]
    public GameObject[] leftWallArtifacts;

    //middle wall artifacts 
    [SerializeField]
    public GameObject[] middleWallArtifacts;

    //right wall artifacts 
    [SerializeField]
    public GameObject[] rightWallArtifacts;

    //banner with instructions for player
    [SerializeField]
    private GameObject wallClickInstructions;

    //to keep track of current wall/view
    private bool onWideShot = true;
    private bool onLeftWall = false;
    private bool onMiddleWall = false;
    private bool onRightWall = false;
    private int currentWallIndex = 0;

    [SerializeField]
    private bool mingming = false; //keep this off AT ALL TIMES 

    void Start()
    {
        GM = FindFirstObjectByType<F26_GameManager>();

        //dont mind this part 
        if (mingming)
        {
            foreach (var wall in clickableWalls)
            {
                wall.GetComponent<Image>().color = new Color(255, 255, 255, 1);
            }
        }
        else
        {
            foreach (var wall in clickableWalls)
            {
                wall.GetComponent<Image>().color = new Color(255, 255, 255, 0);
            }
        }

        //enable wall view exclusive buttons/ui elements
        wallClickInstructions.SetActive(true);
        EnableButton(returnToLobbyButton);
        returnToLobbyButton.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        ToggleButtonArray(true, clickableWalls);
        ToggleWalls();

        //set LevelLoader
        LL = FindFirstObjectByType<LevelLoader>();
        
        //disable wall view exclusive buttons
        DisableButton(leftArrow);
        DisableButton(rightArrow);
        DisableButton(exitWallView);

        //Begin Dialog
        if (GM.CurrentScene.Equals("WarRoomIntro"))
        {
            NextMinigame();
            NextOrder();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (onWideShot)
        {
            //enable wideshot exclusive elements
            EnableButton(returnToLobbyButton);
            ToggleButtonArray(true, clickableWalls);
            returnToLobbyButton.GetComponentInChildren<TextMeshProUGUI>().enabled = true;

            //disable wall view exclusive elements
            DisableButton(leftArrow);
            DisableButton(rightArrow);
            DisableButton(exitWallView); 
        }
        else
        {
            //disable wideshot exclusive buttons
            DisableButton(returnToLobbyButton);
            returnToLobbyButton.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            ToggleButtonArray(false, clickableWalls);
        }
        //set background sprite as current wall 
        ChangeBackgroundSprite(currentWallIndex);
    }

    /// <summary>
    /// changes wall index, current wall boolean, and
    /// currently active artifacts to move to the wall
    /// immediately to the left (loops over to
    /// rightmost wall if already on left wall)
    /// </summary>
    public void MoveLeft()
    {
        currentWallIndex--;

        //loop over to right wall if already on left
        if (currentWallIndex < 1) {
            currentWallIndex = 3;
        }
        MoveWallHelper(); //adjust wall/view tracking booleans
        ToggleWalls(); //disables artifacts of other walls, enables artifacts of current wall
    }

    /// <summary>
    /// changes wall index, current wall boolean, and
    /// currently active artifacts to move to the wall
    /// immediately to the right (loops over to
    /// leftmost wall if already on right wall)
    /// </summary>
    public void MoveRight()
    {
        currentWallIndex++;

        //loop over to left wall if already on right
        if (currentWallIndex > 3)
        {
            currentWallIndex = 1;
        }
        MoveWallHelper(); //adjust wall/view tracking booleans
        ToggleWalls(); //disables artifacts of other walls, enables artifacts of current wall
    }
    /// <summary>
    /// toggles wall tracker booleans based on currentWallIndex
    /// </summary>
    private void MoveWallHelper()
    {
        if (currentWallIndex == 0)
        {
            onWideShot = true;
        }
        else
        {
            onWideShot= false;
        }

        if (currentWallIndex == 1)
        {
            onLeftWall = true;
        }
        else
        {
            onLeftWall = false;
        }

        if (currentWallIndex == 2)
        {
            onMiddleWall = true;
        }
        else
        {
            onMiddleWall = false;
        }

        if (currentWallIndex == 3)
        {
            onRightWall = true;
        }
        else
        {
            onRightWall = false;
        }
    }

    /// <summary>
    /// toggles wall tracker booleans (only onWideShot is true), 
    /// current wall index (set to 0), disables all artifacts,
    /// and changes background to wideshot background
    /// </summary>
    public void ExitWallView()
    {
        onWideShot = true;
        onLeftWall = false;
        onMiddleWall = false;
        onRightWall = false;
        Debug.Log("Exiting wall view...");
        currentWallIndex = 0;

        DisableButton(exitWallView);
        ToggleWalls();
    }

    /// <summary>
    /// changes scenes to museum scene
    /// </summary>
    public void ReturnToLobby()
    {
        Debug.Log("Returning to lobby...");
        if (!GM.completedScenes["WarRoomIntro"])
        {
            GM.completedScenes["WarRoomIntro"] = true;
            GM.CurrentScene = "R+JIntro";
        }
        int museumSceneIndex = 8;
        LL.LoadLevelByBuildIndex(museumSceneIndex, true);
    }

    /// <summary>
    /// disables wideshot exclusive elements, sets
    /// onLeftWall to true (and all other wall tracker
    /// bools to false), enables wall view exclusive buttons,
    /// enables left wall artifacts (and disables all others)
    /// </summary>
    public void LeftWallClicked()
    {
        wallClickInstructions.SetActive(false);
        onWideShot = false;
        onLeftWall = true;
        onMiddleWall = false;
        onRightWall = false;
        currentWallIndex = 1;

        Debug.Log("Left Wall clicked!");
        currentWallIndex = 1;

        EnableButton(exitWallView);
        EnableButton(leftArrow);
        EnableButton(rightArrow);
        ToggleWalls();
    }

    /// <summary>
    /// disables wideshot exclusive elements, sets
    /// onLeftWall to true (and all other wall tracker
    /// bools to false), enables wall view exclusive buttons,
    /// enables center wall artifacts (and disables all others)
    /// </summary>
    public void MiddleWallClicked()
    {
        wallClickInstructions.SetActive(false);
        onWideShot = false;
        onLeftWall = false;
        onMiddleWall = true;
        onRightWall = false;
        currentWallIndex = 2;

        Debug.Log("Middle Wall clicked!");
        currentWallIndex = 2;

        EnableButton(exitWallView);
        EnableButton(leftArrow);
        EnableButton(rightArrow);
        ToggleWalls();
    }

    /// <summary>
    /// disables wideshot exclusive elements, sets
    /// onLeftWall to true (and all other wall tracker
    /// bools to false), enables wall view exclusive buttons,
    /// enables right wall artifacts (and disables all others)
    /// </summary>
    public void RightWallClicked()
    {
        wallClickInstructions.SetActive(false);
        onWideShot = false;
        onLeftWall = false;
        onMiddleWall = false;
        onRightWall = true;
        currentWallIndex = 3;

        Debug.Log("Right Wall clicked!");
        currentWallIndex = 3;

        EnableButton(exitWallView);
        EnableButton(leftArrow);
        EnableButton(rightArrow);
        ToggleWalls();
    }

    /// helper that turns a button interactable and visible
    /// <param name="button">the button to enable</param>
    private void EnableButton(Button button)
    {
        button.interactable = true;
        button.GetComponent<Image>().enabled = true;
    }

    /// helper that turns a button uninteractable and invisible
    /// <param name="button">the button to disable</param>
    private void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().enabled = false;
    }

    /// <summary>
    /// helper that enables all artifacts on current wall
    /// while disabling all others
    /// </summary>
    private void ToggleWalls()
    {
        ToggleArtifacts(onLeftWall, leftWallArtifacts); 
        ToggleArtifacts(onMiddleWall, middleWallArtifacts);   
        ToggleArtifacts(onRightWall, rightWallArtifacts);
    }

    /// <summary>
    /// helper that enables/disables an entire button array
    /// </summary>
    /// <param name="enable">true for enable, false for disable</param>
    /// <param name="buttons">the array of buttons to toggle</param>
    /// <param name="exempt">the button to ignore</param>
    private void ToggleButtonArray(bool enable, Button[] buttons, Button exempt = null)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == exempt)
            {
                continue;
            }
            if (enable)
            {
                EnableButton(buttons[i]);    
            }
            else
            {
                DisableButton(buttons[i]);
                
            }
        }
    }

    /// <summary>
    /// helper that enables/disables an array of artifacts (GameObjects)
    /// </summary>
    /// <param name="enable">true for enable, false for disable</param>
    /// <param name="artifacts">the array of artifacts to toggle</param>
    private void ToggleArtifacts(bool enable, GameObject[] artifacts)
    {
        foreach (var artifact in artifacts)
        {
            artifact.SetActive(enable);
            Button btn = artifact.GetComponentInChildren<Button>(true);
            if (enable)
            {
                EnableButton(btn);
                btn.GetComponent<Image>().raycastTarget = true;
            }
            else
            {
                DisableButton(btn);
                btn.GetComponent<Image>().raycastTarget = false;
               
            }
        }
    }   

}
