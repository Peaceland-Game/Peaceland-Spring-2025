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
    public Button artifact1;

    void Start()
    {
        LL = FindFirstObjectByType<LevelLoader>();
        DisableButton(exitZoomButton); // Disable the exit zoom button at the start
        DisableButton(artifact1); // Disable the artifact button at the start
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToLobby()
    {
        Debug.Log("Returning to lobby...");
        int museumSceneIndex = 2;
        LL.LoadLevelByBuildIndex(museumSceneIndex);
    }

    public void RightWallClicked()
    {
        Debug.Log("Right Wall clicked!");
        DisableButton(returnToLobbyButton); // Disable the return to lobby button
        EnableButton(artifact1); // Enable the artifact button
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

}
