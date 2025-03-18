using UnityEngine;
using System.Collections.Generic;
using Yarn.Unity;

public class FlowerShopManager : MonoBehaviour
{
    /// <summary>
    /// Enum states for the flower memory
    /// </summary>
    public enum FlowerGameState
    {
        START_DIALOGUE = 0,
        DETHORN,
        TRIM,
        ARRANGE,
        END_DIALOGUE
    }

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
    private static int currentFlower = 0;

    /// <summary>
    /// Gives access to the index of the current flower in the current order
    /// </summary>
    public static int CurrentFlower
    {
        get { return currentFlower; }
    }

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

    void Start()
    {
        Instance = this;
        NextMinigame();
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
    }

    /// <summary>
    /// Move on to the next flower in the memory
    /// </summary>
    public void NextFlower()
    {
        currentFlower++;
    }

    /// <summary>
    /// Changes the state of the flower shop memory
    /// </summary>
    /// <param name="state">The enum state to change to</param>
    public void NextMinigame()
    {

        if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();

        //If the current step in the current flower for the current order is not needed, then continue to the next minigame
        if ((!GetCurrentOrder().flowers[currentFlower].needsDethorning && currentMinigame == 1) ||
            (!GetCurrentOrder().flowers[currentFlower].needsTrimming && currentMinigame == 2) ||
            (!GetCurrentOrder().flowers[currentFlower].needsArranging && currentMinigame == 3))
        {
            currentMinigame++;
            NextMinigame();
        }

        currentMinigame++;

        if (currentMinigame >= minigames.Count)
        {
            currentOrder++;
            currentMinigame = 0;
        }
        minigames[currentMinigame].StartMinigame();
    }
}
