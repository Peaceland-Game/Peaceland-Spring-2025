using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RJMemManager : GenericMemManager
{
    /* Evan's Notes:
     * As of 3/27/2026, there are no differing behaviors between this manager and its parent.
     * However, that is likely to change with the introduction of the minigames.
     * In the event that it does not change, this script can be deleted and replaced with GenericMemManager
     *      once it is no longer abstract.
     */

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NextMinigame();
        NextOrder();
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
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
            SceneManager.LoadScene(2);
            return;
        }
        // Otherwise, start the next minigame.
        minigames[currentMinigame].StartMinigame();
    }
}
