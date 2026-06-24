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

    //left wall artifacts 
    [SerializeField]
    public Button[] leftWallArtifacts;

    //middle wall artifacts 
    public Button[] middleWallArtifacts;

    //right wall artifacts 
    public Button[] rightWallArtifacts;

    private bool onWideShot = true;
    private bool onLeftWall = false;
    private bool onMiddleWall = false;
    private bool onRightWall = false;

    void Start()
    {
        LL = FindFirstObjectByType<LevelLoader>();
        DisableButton(exitZoomButton); // Disable the exit zoom button at the start
        ToggleEntireWall(false, leftWallArtifacts); // Disable all left wall artifact buttons at the start
        ToggleEntireWall(false, middleWallArtifacts); // Disable all middle wall artifact buttons at the start  
        ToggleEntireWall(false, rightWallArtifacts); // Disable all right wall artifact buttons at the start

    }

    // Update is called once per frame
    void Update()
    {
        if (!onLeftWall)
        {
            ToggleEntireWall(false, leftWallArtifacts); // Disable all left wall artifact buttons
        }
        if (!onMiddleWall)
        {
            ToggleEntireWall(false, middleWallArtifacts);
        }
        if(!onRightWall)
        {
            ToggleEntireWall(false, rightWallArtifacts);
        }


    }

    public void ReturnToLobby()
    {
        Debug.Log("Returning to lobby...");
        int museumSceneIndex = 2;
        LL.LoadLevelByBuildIndex(museumSceneIndex);
    }
    public void LeftWallClicked()
    {
        onLeftWall = true;
        onMiddleWall = false;
        onRightWall = false;

        Debug.Log("LEft Wall clicked!");
        DisableButton(returnToLobbyButton); // Disable the return to lobby button
        //EnableButton(artifact1); // Enable the artifact button
        ChangeBackgroundSprite(1);
    }

    public void MiddleWallClicked()
    {
        onLeftWall = false;
        onMiddleWall = true;
        onRightWall = false;

        Debug.Log("Middle Wall clicked!");
        DisableButton(returnToLobbyButton); // Disable the return to lobby button
        //EnableButton(artifact1); // Enable the artifact button
        ChangeBackgroundSprite(2);
    }

    public void RightWallClicked()
    {
        onLeftWall = false;
        onMiddleWall = false;
        onRightWall = true;

        Debug.Log("Right Wall clicked!");
        DisableButton(returnToLobbyButton); // Disable the return to lobby button
        ToggleEntireWall(true, rightWallArtifacts); // Enable all right wall artifact buttons
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

    private void ToggleEntireWall(bool enable, Button[] artifacts)
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
