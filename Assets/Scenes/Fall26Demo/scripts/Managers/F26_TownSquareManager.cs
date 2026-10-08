using UnityEngine;
using Unity.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using UnityEditor.Rendering.Universal.ShaderGUI;

public class F26_TownSquareManager : GenericMemManager
{
    [SerializeField]
    private GameObject continueButton;
    //Initializes variables and plays the starting dialogue
    void Start()
    {
        LL = FindFirstObjectByType<LevelLoader>();
        F26_GameManager.Instance.CurrentScene = "Day1TownSquare";
        NextMinigame();
        NextOrder();

    }

    void Update()
    {
        continueButton.SetActive(!dialogueRunner.IsDialogueRunning);
    }

    public void GoHome()
    {
        LL.LoadLevelByBuildIndex(13, true);
    }
}
