using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class F26_PostSneakManager : GenericMemManager
{
    private bool completedSequence = false;
    //Initializes variables and plays the starting dialogue
    void Start()
    {
        F26_GameManager.Instance.CurrentScene = "PostSneak";
        NextMinigame();
        NextOrder();
    }

    public override void NextMinigame()
    {
        if (completedSequence) {
            LevelLoader.LoadNextLevel();
        }
        else
        {
            base.NextMinigame();
            completedSequence = true;
        }
    }
}
