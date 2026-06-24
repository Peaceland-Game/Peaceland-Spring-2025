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

    private bool onWideShot = true;
    private bool onLeftWall = false;
    private bool onMiddleWall = false;
    private bool onRightWall = false;

    void Start()
    {
        LL = FindFirstObjectByType<LevelLoader>();
        DisableButton(exitZoomButton); // Disable the exit zoom button at the start
        DisableButton(exitWallView); // Disable the exit wall view button at the start
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
        if (!onWideShot)
        {
            DisableButton(returnToLobbyButton);
            ToggleButtonArray(false, clickableWalls);
            EnableButton(exitWallView);
        }
        else
        {
            EnableButton(returnToLobbyButton);
            ToggleButtonArray(true, clickableWalls);
        }

    }

    public void ExitWallView()
    {
        onWideShot = true;
        onLeftWall = false;
        onMiddleWall = false;
        onRightWall = false;
        Debug.Log("Exiting wall view...");
        ChangeBackgroundSprite(0);
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

        Debug.Log("LEft Wall clicked!");
        ChangeBackgroundSprite(1);
    }

    public void MiddleWallClicked()
    {
        onWideShot = false;
        onLeftWall = false;
        onMiddleWall = true;
        onRightWall = false;

        Debug.Log("Middle Wall clicked!");
        ChangeBackgroundSprite(2);
    }

    public void RightWallClicked()
    {
        onWideShot = false;
        onLeftWall = false;
        onMiddleWall = false;
        onRightWall = true;

        Debug.Log("Right Wall clicked!");
        ChangeBackgroundSprite(3);
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
