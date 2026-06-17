using System;
using UnityEngine;
using UnityEngine.UI;

public class WarRoom : MonoBehaviour
{
    [SerializeField]
    public Button returnButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private LevelLoader LL;
    void Start()
    {
        LL = FindFirstObjectByType<LevelLoader>();
      
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
}
