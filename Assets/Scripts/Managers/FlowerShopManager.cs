using UnityEngine;
using System.Collections.Generic;
using Yarn.Unity;
using UnityEngine.SceneManagement;

public class FlowerShopManager : MonoBehaviour
{
    /// <summary>
    /// Enum states for the flower memory
    /// </summary>
    //public enum FlowerGameState
    //{
    //    START_DIALOGUE = 0,
    //    DETHORN,
    //    TRIM,
    //    ARRANGE,
    //    END_DIALOGUE
    //}

    /// <summary>
    /// List of "minigames"
    /// </summary>
    [SerializeField]
    private List<MinigameBehavior> minigames;
    private int currentMinigame = -1;

    [SerializeField]
    private List<OrderObject> orders;

    public List<OrderObject> Orders { get => orders; }

    /// <summary>
    /// Index of the current flower in the current order
    /// </summary>
    public static int currentFlower = 0;

    /// <summary>
    /// Singleton Instace
    /// </summary>
    public static FlowerShopManager Instance { get; private set; }

    /// <summary>
    /// Node for random dialogue when doing well.
    /// </summary>
    [SerializeField]
    private string gameplayGoodNode;

    [SerializeField]
    private DialogueRunner dialogueRunner;

    /// <summary>
    /// List of sprites associated with flower types
    /// </summary>
    [SerializeField]
    private Sprite[] flowerTopSprites;

    [SerializeField]
    private Sprite[] flowerBottomSprites;

    /// <summary>
    /// The current order being worked on
    /// </summary>
    private static int currentOrder = 0;

    /// <summary>
    /// Gives access to the current order in the minigame
    /// </summary>
    /// <returns>The current order being worked on</returns>
    public static OrderObject GetCurrentOrder()
    {
        return Instance.orders[currentOrder];
    }

    /// <summary>
    /// Increment the current order and go to the next one
    /// </summary>
    public static void NextOrder()
    {
        currentOrder++;
    }

    /// <summary>
    /// Increment the current order and grab the sprite in it
    /// </summary>
    public static Sprite NextOrderChar()
    {
        currentOrder++;
        return GetCurrentOrder().spriteForOrder;
    }

    /// <summary>
    /// The index of the current minigame that this memory is on
    /// </summary>
    /// <returns>The index of the current minigame this memory is on</returns>
    public static int GetCurrentMinigameIndex()
    {
        return Instance.currentMinigame;
    }

    /// <summary>
    /// The current minigame that this memory is on
    /// </summary>
    /// <returns>The current minigame this memory is on</returns>
    public static MinigameBehavior GetCurrentMinigame()
    {
        return Instance.minigames[GetCurrentMinigameIndex()];
    }

    /// <summary>
    /// The current flower that this memory is on
    /// </summary>
    /// <param name="flowerNum">The index to search for in the flowers list in the current order (-1 uses the current flower 
    /// in the FlowerShopManager instance)</param>
    /// <returns>The current flower in the current order</returns>
    public static OrderObject.Flower GetCurrentFlower(int flowerNum = -1)
    {
        if (flowerNum < 0)
        {
            return GetCurrentOrder().flowers[currentFlower];
        }
        else
        {
            return GetCurrentOrder().flowers[flowerNum];
        }
    }

    /// <summary>
    /// Gets the correct flower sprite that matches the given flower type
    /// </summary>
    /// <param name="t">The given flower type to search for</param>
    /// <returns>The flower sprite that matches the given one</returns>
    public static Sprite GetFlowerTopSprite(OrderObjectType t)
    {
        return Instance.flowerTopSprites[(int)t];
    }

    public static Sprite GetFlowerBottomSprite(OrderObjectType t)
    {
        return Instance.flowerBottomSprites[(int)t];
    }

    public static void ResetFlowerShop()
    {
        currentFlower = 0;
        currentOrder = 0;
    }

    void Start()
    {
        Instance = this;
        NextMinigame();
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
    }

    /// <summary>
    /// Changes the state of the flower shop memory
    /// </summary>
    /// <param name="state">The enum state to change to</param>
    public void NextMinigame()
    {
        //As long as the current minigame is at a valid index (greater than 0), stop the minigame at that index
        if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();

        //Increment to the next minigame
        currentMinigame++;

        //If the current minigame is higher or equal to the number of minigames, continue
        if (currentMinigame >= minigames.Count)
        {
            SceneManager.LoadScene(2);
            return;

            // TODO: We're done, end the game (or memory)!!!
        }

        //Start the next minigame
        minigames[currentMinigame].StartMinigame();
    }
}
