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
    //public Button[] leftWallArtifacts;
    public GameObject[] leftWallArtifacts;

    //middle wall artifacts 
    [SerializeField]
    public GameObject[] middleWallArtifacts;

    //right wall artifacts 
    [SerializeField]
    public GameObject[] rightWallArtifacts;

    [SerializeField]
    private GameObject wallClickInstructions;

    //[SerializeField]
    //public Button exitDescView;

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

    [SerializeField]
    private bool mingming = false;

    //private Button[] leftWallArtifactButtons;
    //private Button[] middleWallArtifactButtons;
    //private Button[] rightWallArtifactButtons;

    void Start()
    {
        //for (int i = 0; i < leftWallArtifacts.Length; i++)
        //{
        //    leftWallArtifactButtons[i] = leftWallArtifacts[i].GetComponentInChildren<Button>(true);
        //}
        //for (int i = 0; i < middleWallArtifacts.Length; i++)
        //{
        //    middleWallArtifactButtons[i] = middleWallArtifacts[i].GetComponentInChildren<Button>(true);
        //}
        //for (int i = 0; i < rightWallArtifacts.Length; i++)
        //{
        //    rightWallArtifactButtons[i] = rightWallArtifacts[i].GetComponentInChildren<Button>(true);
        //}

        wallClickInstructions.SetActive(true);

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

        GM = FindFirstObjectByType<GameManager>();
        LL = FindFirstObjectByType<LevelLoader>();
        EnableButton(returnToLobbyButton);
        returnToLobbyButton.GetComponentInChildren<TextMeshProUGUI>().enabled = true;

        DisableButton(leftArrow);
        DisableButton(rightArrow);
        DisableButton(exitWallView); // Disable the exit wall view button at the start

        ToggleButtonArray(true, clickableWalls);

        ToggleWalls();

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
            returnToLobbyButton.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
            DisableButton(leftArrow);
            DisableButton(rightArrow);
            DisableButton(exitWallView); // Disable the exit wall view button at the start
            ToggleButtonArray(true, clickableWalls);
        }
        else
        {
            DisableButton(returnToLobbyButton);
            returnToLobbyButton.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
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

        //stupid hardcode fix that i  hope never gets uncovered by a future developer
        //rightWallArtifacts[5].GetComponentInChildren<Button>(true).GetComponent<Image>().enabled=true;
       // Debug.Log("Enabling artifact button through fuckahh hardcode: " + rightWallArtifacts[5].GetComponentInChildren<Button>(true).name);
    }

    private void EnableButton(Button button)
    {
        button.interactable = true;
        button.GetComponent<Image>().enabled = true;
    }

    private void ToggleWalls()
    {
        ToggleArtifacts(onLeftWall, leftWallArtifacts); 
        ToggleArtifacts(onMiddleWall, middleWallArtifacts);   
        ToggleArtifacts(onRightWall, rightWallArtifacts);
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
            //if (buttons[i] == exempt) {
                //continue;
            //}
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

    private void ToggleArtifacts(bool enable, GameObject[] artifacts)
    {
        foreach (var artifact in artifacts)
        {
            artifact.SetActive(enable);
            Button btn = artifact.GetComponentInChildren<Button>(true);
            Debug.Log("Toggling artifact button: " + btn.name + " to " + enable);
            if (enable)
            {
                EnableButton(btn);
                btn.GetComponent<Image>().raycastTarget = true;
                Debug.Log("Enabling artifact button: " + btn.name);
            }
            else
            {
                DisableButton(btn);
                btn.GetComponent<Image>().raycastTarget = false;
               
            }
        }
    }   

}
