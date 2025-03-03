using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerShopDialogue))]
public class FlowerManager : MonoBehaviour
{
    /// <summary>
    /// Dialogue manager for the flower memory
    /// </summary>
    private FlowerShopDialogue dialogueManager;

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

    /// <summary>
    /// The canvas that holds the checklist
    /// </summary>
    [SerializeField]
    private GameObject checklistCanvas;

    [SerializeField]
    private List<OrderObject> orders;

    public List<OrderObject> Orders { get => orders; }

    public static FlowerManager Instance { get; private set; }

    public static OrderObject GetCurrentOrder()
    {
        return Instance.orders[0];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        dialogueManager = GetComponent<FlowerShopDialogue>();
        ChangeState(FlowerGameState.START_DIALOGUE);
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
    /// Changes the state of the enum
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
                dialogueManager.DoIntroDialogue();
                dethornGame.SetActive(false);
                trimGame.SetActive(false);
                arrangeGame.SetActive(false);
                checklistCanvas.SetActive(false);
                break;
            case FlowerGameState.DETHORN:
                dethornGame.SetActive(true);
                trimGame.SetActive(false);
                arrangeGame.SetActive(false);
                checklistCanvas.SetActive(true);
                break;
            case FlowerGameState.TRIM:
                dethornGame.SetActive(false);
                trimGame.SetActive(true);
                arrangeGame.SetActive(false);
                checklistCanvas.SetActive(true);
                break;
            case FlowerGameState.ARRANGE:
                dethornGame.SetActive(false);
                trimGame.SetActive(false);
                arrangeGame.SetActive(true);
                checklistCanvas.SetActive(true);
                break;
            case FlowerGameState.END_DIALOGUE:
                dethornGame.SetActive(false);
                trimGame.SetActive(false);
                arrangeGame.SetActive(false);
                checklistCanvas.SetActive(false);
                break;
            default:
                break;
        }
    }
}
