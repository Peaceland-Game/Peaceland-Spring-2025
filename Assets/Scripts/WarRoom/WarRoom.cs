using System;
using UnityEngine;
using UnityEngine.UI;

public class WarRoom : MonoBehaviour
{
    [SerializeField]
    public Button returnToLobbyButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    public Button exitZoomButton;

    private LevelLoader LL;
    void Start()
    {
        LL = FindFirstObjectByType<LevelLoader>();
        DisableButton(exitZoomButton); // Disable the exit zoom button at the start

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

    private void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().enabled = false;
    }
}
