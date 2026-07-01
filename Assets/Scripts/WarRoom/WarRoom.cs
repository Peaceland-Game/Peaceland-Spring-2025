using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarRoom : GenericMemManager
{
    [SerializeField]
    public Button returnToLobbyButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    public Button exitWallView;

    [SerializeField]
    public Button leftArrow;

    [SerializeField]
    public Button rightArrow;

    [SerializeField]
    public Button[] clickableWalls;

    //left wall artifacts 
    [SerializeField]
    public Button[] leftWallArtifacts;

    //middle wall artifacts 
    [SerializeField]
    public Button[] middleWallArtifacts;

    //right wall artifacts 
    [SerializeField]
    public Button[] rightWallArtifacts;

    [SerializeField]
    public Button exitDescView;

    ////left wall artifacts 
    //[SerializeField]
    //public TextMeshProUGUI[] leftWallTMPs;

    ////middle wall artifacts 
    //[SerializeField]
    //public TextMeshProUGUI[] middleWallTMPs;

    ////right wall artifacts 
    //[SerializeField]
    //public TextMeshProUGUI[] rightWallTMPs;

    //private bool[] currentWall = new bool[4];
    private bool onWideShot = true;
    private bool onLeftWall = false;
    private bool onMiddleWall = false;
    private bool onRightWall = false;
    
    private int currentWallIndex = 0;

    private GameManager GM;

    void Start()
    {
        GM = FindFirstObjectByType<GameManager>();
        Debug.Log("seen rj: "+GameManager.Instance.seenRJMemory);
        LL = FindFirstObjectByType<LevelLoader>();
        EnableButton(returnToLobbyButton);

        DisableButton(leftArrow);
        DisableButton(rightArrow);
        DisableButton(exitWallView); // Disable the exit wall view button at the start

        ToggleButtonArray(true, clickableWalls);

        ToggleButtonArray(false, leftWallArtifacts); // Disable all left wall artifact buttons at the start
        ToggleButtonArray(false, middleWallArtifacts); // Disable all middle wall artifact buttons at the start  
        ToggleButtonArray(false, rightWallArtifacts); // Disable all right wall artifact buttons at the start

    }

    // Update is called once per frame
    void Update()
    {
        //ToggleButtonArray(onLeftWall, leftWallArtifacts); // Disable all left wall artifact buttons at the start
        //ToggleButtonArray(onMiddleWall, middleWallArtifacts); // Disable all middle wall artifact buttons at the start  
        //ToggleButtonArray(onRightWall, rightWallArtifacts); // Disable all right wall artifact buttons at the start
        if (onWideShot)
        {
            EnableButton(returnToLobbyButton);
            DisableButton(leftArrow);
            DisableButton(rightArrow);
            DisableButton(exitWallView); // Disable the exit wall view button at the start
            ToggleButtonArray(true, clickableWalls);
        }
        else
        {
            DisableButton(returnToLobbyButton);
            ToggleButtonArray(false, clickableWalls);
        }
        ChangeBackgroundSprite(currentWallIndex);
    }

    public void MoveLeft()
    {
        currentWallIndex--;
        if (currentWallIndex < 1) {
            currentWallIndex = 3;
        }
        MoveWallHelper();
        ToggleWalls();
    }

    public void MoveRight()
    {
        currentWallIndex++;
        if (currentWallIndex > 3)
        {
            currentWallIndex = 1;
        }
        MoveWallHelper();
        ToggleWalls();
    }

    //don't ask
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

    public void ReturnToLobby()
    {
        Debug.Log("Returning to lobby...");
        int museumSceneIndex = 2;
        LL.LoadLevelByBuildIndex(museumSceneIndex);
    }
    public void LeftWallClicked()
    {
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

    public void MiddleWallClicked()
    {
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

    public void RightWallClicked()
    {
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

    private void EnableButton(Button button)
    {
        button.interactable = true;
        button.GetComponent<Image>().enabled = true;
    }

    private void ToggleWalls()
    {
        ToggleButtonArray(onLeftWall, leftWallArtifacts); // Disable all left wall artifact buttons at the start
        ToggleButtonArray(onMiddleWall, middleWallArtifacts); // Disable all middle wall artifact buttons at the start  
        ToggleButtonArray(onRightWall, rightWallArtifacts); // Disable all right wall artifact buttons at the start
    }
    private void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().enabled = false;
    }

    private void ToggleButtonArray(bool enable, Button[] buttons, Button exempt = null)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == exempt) {
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

}
