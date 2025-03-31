using UnityEngine;
using System.Collections.Generic;
using Yarn.Unity;

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
    private Sprite[] flowerSprites;

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

    public static void NextOrder() {
        currentOrder++;
    }

    /// <summary>
    /// The current minigame that this memory is on
    /// </summary>
    /// <returns>The current minigame this memory is on</returns>
    public static int GetCurrentMinigame()
    {
        return Instance.currentMinigame;
    }

    /// <summary>
    /// The current flower that this memory is on
    /// </summary>
    /// <returns>The current flower in the current order</returns>
    public static OrderObject.Flower GetCurrentFlower(int flowerNum)
    {
        return GetCurrentOrder().flowers[flowerNum];
    }

    /// <summary>
    /// Returns the appropriate part of the flower, based on the current minigame
    /// </summary>
    /// <param name="index">The current flower to pull the appropriate game object from</param>
    /// <param name="minigame">The current minigame to test for</param>
    /// <returns>Flower stem if the minigame is dethorning, and whole flower if the minigame is trimming</returns>
    public static GameObject ReturnGameObjectBasedOnMinigame(int index, int minigame)
    {
        //If the minigame is dethorning, return the flower stem
        if (minigame == 1)
        {
            return GetCurrentOrder().flowers[index].flowerStem;
        }
        //Otherwise return the entire flower game object
        else
        {
            return GetCurrentOrder().flowers[index].flowerObject;
        }
    }

    public static Sprite GetFlowerSprite(FlowerType t)
    {
        return Instance.flowerSprites[(int)t];
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

        if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();

        //Increment to the next minigame
        currentMinigame++;

        //If the current minigame is higher or equal to the number of minigames, continue
        if (currentMinigame >= minigames.Count)
        {
            currentMinigame = 0;
            // TODO: W're done, end the game (or memory)!!!
        }

        //Start the next minigame
        minigames[currentMinigame].StartMinigame();
    }
}
