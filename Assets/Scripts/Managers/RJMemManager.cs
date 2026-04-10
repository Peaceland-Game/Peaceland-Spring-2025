using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RJMemManager : GenericMemManager
{
    [SerializeField]
    private GameObject cakeSprite;  // The cousin's cake
    // NOTE: currently no good way to show and hide cake when the cousin enters, just leaving it in the scene
        // Could add a new dialogue minigame for when the cousin enters

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        // Hide cake asset when moving into puzzle minigame
        if (currentMinigame == 1)
        {
            cakeSprite.SetActive(false);
        }

        // Increment minigame counter
        currentMinigame++;

        // If passed all the minigames, reset the Unity scene and go to the next Unity scene
        if (currentMinigame >= minigames.Count)
        {
            ResetMemory();
            Debug.Log("End Memory");

            // Update GM bool and return to the present
            GameManager.Instance.seenRJMemory = true;
            LL.LoadLevelByBuildIndex(2);
            return;
        }

        // Otherwise, start the next minigame.
        minigames[currentMinigame].StartMinigame();
    }
}
