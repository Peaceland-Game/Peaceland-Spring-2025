using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RJMemManager : GenericMemManager
{
    // Fields
    // Minigame trackers
    //[SerializeField]
    //private List<MinigameBehavior> minigames;
    //private int currentMinigameIndex = -1;

    ///// <summary>
    ///// Returns the current minigame
    ///// </summary>
    //public MinigameBehavior CurrentMinigame { get { return minigames[currentMinigameIndex]; } }

    ///// <summary>
    ///// Returns the index of the current minigame
    ///// </summary>
    //public int CurrentMinigameIndex { get { return currentMinigameIndex; } }

    //// Dialogue system and sprites
    //[SerializeField]
    //private DialogueRunner dialogueRunner;
    //[SerializeField]
    //private List<OrderObject> orders; // Only used until a new portrait storage system is created
    //private int currentOrder = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NextMinigame();
        NextOrder();
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
    }

    /// <summary>
    /// Iterate to the next minigame.
    /// </summary>
    public override void NextMinigame()
    {
        Debug.Log("Current Minigame " + currentMinigame);

        if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();

        currentMinigame++;

        // If passed all the minigames, reset the scene and go to the next scene
        if (currentMinigame >= minigames.Count)
        {
            ResetMemory();
            Debug.Log("End R&J Memory test Test");
            // TODO: Uncomment this line once there is a scene to go to
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }
        // Otherwise, start the next minigame.
        minigames[currentMinigame].StartMinigame();
    }

    /// <summary>
    /// Resets the values of the memory so it can run again
    /// </summary>
    public void ResetMemory()
    {
        currentMinigame = -1;
        currentOrder = -1;
    }

    /// <summary>
    /// Returns the current order object.
    /// </summary>
    /// <returns>The current order object in the list.</returns>
    public override SceneCharatcersObject GetCurrentOrder()
    {
        return scenes[currentOrder];
    }

    /// <summary>
    /// Increments to the next order in the list
    /// </summary>
    public override void NextOrder()
    {
        if (currentOrder + 1 < scenes.Count) currentOrder++;
        else Debug.Log($"Attempted to increment to order {scenes.Count + 1} when there are only {scenes.Count} orders.");
    }

    /// <summary>
    /// Fetches the Main Character's sprites
    /// </summary>
    /// <returns>An array of Unity Sprite Obejcts.</returns>
    public override Sprite[] GetMainSprites()
    {
        return GetCurrentOrder().MainCharSprites;
    }

    /// <summary>
    /// Fetches the Secondary Character's sprites
    /// </summary>
    /// <returns>An array of Unity Sprite Obejcts.</returns>
    public override Sprite[] GetSecondSprites()
    {
        return GetCurrentOrder().SecondCharSprites;
    }

    /// <summary>
    /// Returns the index of the current minigame in the list
    /// </summary>
    /// <returns>Returns the index in the list of minigames.</returns>
    public override int GetCurrentMinigameIndex()
    {
        return currentMinigame;
    }

    /// <summary>
    /// Returns the current minigame object in the list.
    /// </summary>
    /// <returns>Returns the current minigame objct.</returns>
    public override MinigameBehavior GetCurrentMinigame()
    {
        return minigames[currentMinigame];
    }
}
