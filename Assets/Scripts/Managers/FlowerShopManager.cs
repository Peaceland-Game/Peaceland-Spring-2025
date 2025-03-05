using System;
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
    /// Var to keep track of the current state the game is in
    /// </summary>
    private FlowerGameState currentState = FlowerGameState.START_DIALOGUE;

    //GameObjects holding the different aspects of the game to turn on and off
    [SerializeField]
    private GameObject dethornGame;
    [SerializeField]
    private GameObject trimGame;
    [SerializeField]
    private GameObject arrangeGame;

    [SerializeField]
    private List<OrderObject> orders;

    public List<OrderObject> Orders { get => orders; }

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

    private static int currentOrder = 0;

    public static OrderObject GetCurrentOrder()
    {
        return Instance.orders[currentOrder];
    }

    void Start()
    {
        Instance = this;
        ChangeState(FlowerGameState.START_DIALOGUE);
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(UpdateStateFromDialogue);
    }

    /// <summary>
    /// Gives Unity access to changing the state of the enum using a given int
    /// </summary>
    /// <param name="state_num">An int the enum state to change to</param>
    public void ChangeStateInt(int state_num)
    {
        ChangeState((FlowerGameState)state_num);
    }

    /// <summary>
    /// Update state when coming out of dialogue
    /// </summary>
    public void UpdateStateFromDialogue() {
        switch(currentState) {
            case FlowerGameState.START_DIALOGUE:
                ChangeState(FlowerGameState.DETHORN);
                break;
            case FlowerGameState.END_DIALOGUE:
                ChangeState(FlowerGameState.START_DIALOGUE);
                break;
        }
    }


    /// <summary>
    /// Play the intro dialogue
    /// </summary>
    private void DoIntroDialogue()
    {
        dialogueRunner.StartDialogue(GetCurrentOrder().dialogueStartNode);
    }

    /// <summary>
    /// Play the outro dialogue
    /// </summary>
    private void DoOutroDialogue()
    {
        dialogueRunner.StartDialogue(GetCurrentOrder().dialogueEndNode);
    }

    /// <summary>
    /// Changes the state of the flower shop memory
    /// </summary>
    /// <param name="state">The enum state to change to</param>
    public void ChangeState(FlowerGameState state)
    {
        //Switch the current state to the given one
        currentState = state;

        //Handle the new current state
        switch (currentState)
        {
            case FlowerGameState.START_DIALOGUE:
                dethornGame.SetActive(false);
                trimGame.SetActive(false);
                arrangeGame.SetActive(false);
                DoIntroDialogue();
                break;
            case FlowerGameState.DETHORN:
                dethornGame.SetActive(true);
                trimGame.SetActive(false);
                arrangeGame.SetActive(false);
                break;
            case FlowerGameState.TRIM:
                dethornGame.SetActive(false);
                trimGame.SetActive(true);
                arrangeGame.SetActive(false);
                break;
            case FlowerGameState.ARRANGE:
                dethornGame.SetActive(false);
                trimGame.SetActive(false);
                arrangeGame.SetActive(true);
                break;
            case FlowerGameState.END_DIALOGUE:
                dethornGame.SetActive(false);
                trimGame.SetActive(false);
                arrangeGame.SetActive(false);
                DoOutroDialogue();
                // TODO: prevent this from going above # of orders
                currentOrder++;
                break;
            default:
                break;
        }
    }
}
