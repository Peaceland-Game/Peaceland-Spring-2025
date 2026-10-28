using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class F26_EndingManager : GenericMemManager
{
    const int ENDING_THRESHOLD_KNOWLEDGE = 8;
    //Initializes variables and plays the starting dialogue
    void Start()
    {
        F26_GameManager.Instance.CurrentScene = "Day1Home";
        LL = FindFirstObjectByType<LevelLoader>();
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        dialogueRunner.onDialogueComplete.RemoveListener(NextMinigame);
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
        NextMinigame();
        NextOrder();
    }

    void Update()
    {
    }

    public override void NextMinigame()
    {
        //play first section normally
        if (currentMinigame == -1)
        {
            base.NextMinigame();
        }
        //pick correct ending
        else if (currentMinigame == 0)
        {
            Debug.Log("ajshdkjashdk");
            if(F26_GameManager.Instance.KnowledgeStat >= ENDING_THRESHOLD_KNOWLEDGE)
            {
                currentMinigame = 1;
            }
            else
            {
                currentMinigame = 2;
            }
            minigames[currentMinigame].StartMinigame();
        }
        //after ending, reset game manager and go to main menu
        else
        {
            F26_GameManager.Instance.ResetGameManager();
            LL.LoadLevelByBuildIndex(F26_GameManager.DemoStartBuildIndex);
        }
    }
}
