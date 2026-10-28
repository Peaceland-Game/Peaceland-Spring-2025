using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class F26_PostSneakManager : GenericMemManager
{
    //Initializes variables and plays the starting dialogue
    void Start()
    {
        F26_GameManager.Instance.CurrentScene = "PostSneak";
        LL = FindFirstObjectByType<LevelLoader>();
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
        NextMinigame();
        NextOrder();
    }

    public override void NextMinigame()
    {
        if (currentMinigame >= minigames.Count - 1) {
            LL.LoadLevelByBuildIndex(F26_GameManager.LetterBuildIndex, true);
        }
        else
        {
            base.NextMinigame();
        }
    }
}
