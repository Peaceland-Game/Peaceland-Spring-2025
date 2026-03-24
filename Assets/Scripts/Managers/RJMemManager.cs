using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RJMemManager : MonoBehaviour
{
    // Fields
    // Minigame trackers
    [SerializeField]
    private List<MinigameBehavior> minigames;
    private int currentMinigameIndex = -1;

    /// <summary>
    /// Returns the current minigame
    /// </summary>
    public MinigameBehavior CurrentMinigame { get { return minigames[currentMinigameIndex]; } }

    /// <summary>
    /// Returns the index of the current minigame
    /// </summary>
    public int CurrentMinigameIndex { get { return currentMinigameIndex; } }

    [SerializeField]
    private DialogueRunner dialogueRunner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NextMinigame();
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
    }

    /// <summary>
    /// Iterate to the next minigame.
    /// </summary>
    public void NextMinigame()
    {
        Debug.Log("Current Minigame " + currentMinigameIndex);

        if (currentMinigameIndex >= 0) minigames[currentMinigameIndex].StopMinigame();

        currentMinigameIndex++;

        // If passed all the minigames, reset the scene and go to the next scene
        if (currentMinigameIndex >= minigames.Count)
        {
            ResetMemory();
            Debug.Log("End R&J Memory test Test");
            // TODO: Uncomment this line once there is a scene to go to
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }
        // Otherwise, start the next minigame.
        minigames[currentMinigameIndex].StartMinigame();
    }

    public void ResetMemory()
    {
        currentMinigameIndex = -1;
    }
}
