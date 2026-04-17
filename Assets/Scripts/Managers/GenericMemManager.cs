using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public abstract class GenericMemManager : MonoBehaviour
{
    // List of minigames
    [SerializeField]
    protected List<MinigameBehavior> minigames;
    protected int currentMinigame = -1;
    public int CurrentMinigame { get { return currentMinigame; }}

    // List of "orders" (or other portait storage method)
    [SerializeField]
    protected List<SceneCharatcersObject> scenes;
    protected int currentOrder = -1;
    public int CurrentOrder { get { return currentOrder; } }

    // List of backgrounds for dialogue segments in a memory
    [SerializeField]
    protected Sprite[] backgroundList;
    public Sprite[] BackgroundList { get { return backgroundList; } }
    [SerializeField]
    protected UnityEngine.UI.Image backgroundSprite;

    // Dialogue Runner
    [SerializeField]
    protected DialogueRunner dialogueRunner;

    // Level Loader
    protected LevelLoader LL;
    public LevelLoader LevelLoader { get { return LL; } }

    // Functions
    /// <summary>
    /// Gets the current order in the minigame
    /// </summary>
    /// <returns></returns>
    public virtual SceneCharatcersObject GetCurrentOrder()
    {
        return scenes[currentOrder];
    }

    /// <summary>
    /// Increments to the next OrderObject
    /// </summary>
    public virtual void NextOrder()
    {
        currentOrder++;
    }

    /// <summary>
    /// Fetches the list containing the main character's sprites
    /// </summary>
    /// <returns>A list of Unity Sprite objects</returns>
    public virtual Sprite[] GetMainSprites()
    {
        return scenes[currentOrder].MainCharSprites;
    }

    /// <summary>
    /// Fetches the list containing the secondary character's sprites
    /// </summary>
    /// <returns>A list of Unity Sprite objects</returns>
    public virtual Sprite[] GetSecondSprites()
    {
        return scenes[currentOrder].SecondCharSprites;
    }

    /// <summary>
    /// Gets the current minigame's index in the minigame list
    /// </summary>
    /// <returns>An integer of the index in the minigame list</returns>
    public virtual int GetCurrentMinigameIndex()
    {
        return currentMinigame;
    }

    /// <summary>
    /// Gets the current minigame.
    /// </summary>
    /// <returns>The MinigameBehavior object.</returns>
    public virtual MinigameBehavior GetCurrentMinigame()
    {
        return minigames[currentMinigame];
    }

    /// <summary>
    /// Increments to the next minigame in the list.
    /// </summary>
    public virtual void NextMinigame()
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
            // TODO: Uncomment this line once there is a scene to go to
            //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }
        // Otherwise, start the next minigame.
        minigames[currentMinigame].StartMinigame();
    }

    /// <summary>
    /// Behavior to reset the memory sequence
    /// </summary>
    public virtual void ResetMemory()
    {
        currentMinigame = -1;
        currentOrder = -1;
    }

    /// <summary>
    /// Changes the current background sprite
    /// </summary>
    /// <param name="newBgIndex">The index of the new background sprite to use</param>
    public virtual void ChangeBackgroundSprite(int newBgIndex)
    {
        // Do not set the background if the index is out of range
        if (newBgIndex >= backgroundList.Length)
        {
            Debug.Log($"Index {newBgIndex} is larger than {backgroundList.Length}, cannot set background.");
            return;
        }

        //backgroundSprite.GetComponent<SpriteRenderer>().sprite = backgroundList[newBgIndex];
        backgroundSprite.GetComponent<Image>().sprite = backgroundList[newBgIndex];
    }
}
