using System.Collections.Generic;
using UnityEngine;
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
    protected List<OrderObject> orders;
    protected int currentOrder = -1;
    public int CurrentOrder { get { return currentOrder; } }

    // Dialogue Runner
    [SerializeField]
    protected DialogueRunner dialogueRunner;


    // Functions
    /// <summary>
    /// Gets the current order in the minigame
    /// </summary>
    /// <returns></returns>
    public abstract OrderObject GetCurrentOrder();

    /// <summary>
    /// Increments to the next OrderObject
    /// </summary>
    public abstract void NextOrder();

    /// <summary>
    /// Fetches the list containing the main character's sprites
    /// </summary>
    /// <returns>A list of Unity Sprite objects</returns>
    public abstract Sprite[] GetMainSprites();

    /// <summary>
    /// Fetches the list containing the secondary character's sprites
    /// </summary>
    /// <returns>A list of Unity Sprite objects</returns>
    public abstract Sprite[] GetSecondSprites();

    /// <summary>
    /// Gets the current minigame's index in the minigame list
    /// </summary>
    /// <returns>An integer of the index in the minigame list</returns>
    public abstract int GetCurrentMinigameIndex();

    /// <summary>
    /// Gets the current minigame.
    /// </summary>
    /// <returns>The MinigameBehavior object.</returns>
    public abstract MinigameBehavior GetCurrentMinigame();

    /// <summary>
    /// Increments to the next minigame in the list.
    /// </summary>
    public abstract void NextMinigame();
}
