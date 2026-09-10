using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RJMemManager : GenericMemManager
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < objectImageList.Length; i++)
        {
            HideObjectImage(i);
        }

        NextMinigame();
        NextOrder();
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
        LL = FindFirstObjectByType<LevelLoader>();
    }

    /// <summary>
    /// Override methods to go to the next minigame. The logic is mostly identical
    /// to the base class, but with an added line to check a bool in GameManager
    /// to avoid repeating dialogue in the present.
    /// </summary>
    public override void NextMinigame()
    {
        Debug.Log("Current Minigame " + currentMinigame);

        // Stope the current mingame, if there is one.
        if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();

        // Increment minigame counter
        currentMinigame++;

        // If passed all the minigames, reset the Unity scene and go to the next Unity scene
        if (currentMinigame >= minigames.Count)
        {
            ResetMemory();
            Debug.Log("End Memory");

            // Update GM bool and return to the present
            GameManager.Instance.seenRJMemory = true;
            LL.LoadLevelByBuildIndex(8);
            return;
        }

        // Otherwise, start the next minigame.
        minigames[currentMinigame].StartMinigame();
    }
}
