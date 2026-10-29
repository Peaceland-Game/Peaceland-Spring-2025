using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class F26_HouseManager : GenericMemManager
{

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

    public override void NextMinigame()
    {
        if(currentMinigame < minigames.Count - 1)
        {
            base.NextMinigame();
        }
        else
        {
            LL.LoadLevelByBuildIndex(F26_GameManager.JournalBuildIndex, true);
        }
    }
}
