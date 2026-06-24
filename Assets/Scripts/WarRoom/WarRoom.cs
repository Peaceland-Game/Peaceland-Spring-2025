using System;
using UnityEngine;
using UnityEngine.UI;

public class WarRoom : GenericMemManager
{
    [SerializeField]
    public Button returnToLobbyButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    public Button exitZoomButton;

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

    //private bool[] currentWall = new bool[4];
    private bool onWideShot = true;
    private bool onLeftWall = false;
    private bool onMiddleWall = false;
    private bool onRightWall = false;
    
    private int currentWallIndex = 0;

    void Start()
    {
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
        ToggleButtonArray(onLeftWall, leftWallArtifacts);
        ToggleButtonArray(onMiddleWall, middleWallArtifacts);
        ToggleButtonArray(onRightWall, rightWallArtifacts);
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
            EnableButton(leftArrow);
            EnableButton(rightArrow);
            ToggleButtonArray(false, clickableWalls);
            EnableButton(exitWallView);
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
    }

    public void MoveRight()
    {
        currentWallIndex++;
        if (currentWallIndex > 3)
        {
            currentWallIndex = 1;
        }
        MoveWallHelper();
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
    }

    private void EnableButton(Button button)
    {
        button.interactable = true;
        button.GetComponent<Image>().enabled = true;
    }

    private void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().enabled = false;
    }

    private void ToggleButtonArray(bool enable, Button[] artifacts)
    {
        for (int i = 0; i < artifacts.Length; i++)
        {
            if (enable)
            {
                EnableButton(artifacts[i]);
            }
            else
            {
                DisableButton(artifacts[i]);
            }
        }
    }

}
