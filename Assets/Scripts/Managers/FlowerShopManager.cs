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

    /// <summary>
    /// Increment the current order and go to the next one
    /// </summary>
    public static void NextOrder()
    {
        currentOrder++;
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
    public static GameObject ReturnGameObjectBasedOnMinigame(int index, string minigame)
    {
        //If the minigame is dethorning, return the flower stem
        if (minigame == "Dethorn")
        {
            return GetCurrentOrder().flowers[index].flowerStem;
        }
        //Otherwise return the entire flower game object
        else
        {
            return GetCurrentOrder().flowers[index].flowerObject;
        }
    }

    /// <summary>
    /// Gets the correct flower sprite that matches the given flower type
    /// </summary>
    /// <param name="t">The given flower type to search for</param>
    /// <returns>The flower sprite that matches the given one</returns>
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
        //As long as the current minigame is at a valid index (greater than 0), stop the minigame at that index
        if (currentMinigame >= 0 && currentFlower == 0) minigames[currentMinigame].StopMinigame();

        //If the current step in the current flower for the current order is not needed, then continue to the next minigame
        if ((!GetCurrentFlower(currentFlower).needsDethorning && minigames[currentMinigame + 1].gameObject.name == "Dethorn") ||
            (!GetCurrentFlower(currentFlower).needsTrimming && minigames[currentMinigame + 1].gameObject.name == "Trimming"))
        {
            //Increment the current flower this minigame is on
            currentFlower++;

            //If the next flower is out of range, then go back to the first flower
            if (currentFlower >= GetCurrentOrder().flowers.Count)
            {
                currentFlower = 0;
                CutManager.CurIndex = currentFlower;
            }
            else
            {
                //Update the flower index in the CutManager, and then set it up for cutting
                CutManager.CurIndex = currentFlower;
                CutManager.CutStart();
            }

            NextMinigame();
        }

        //Increment to the next minigame
        currentMinigame++;

        //If the current minigame is higher or equal to the number of minigames, continue
        if (currentMinigame >= minigames.Count)
        {
            //Reset the current minigame num, the current order num, and the current flower num (basically reset everything back
            //to the beginning of the memory
            currentMinigame = 0;
            currentOrder = 0;
            currentFlower = 0;
            CutManager.CurIndex = currentFlower;
            // TODO: W're done, end the game (or memory)!!!
        }

        //Start the next minigame
        minigames[currentMinigame].StartMinigame();
    }
}
