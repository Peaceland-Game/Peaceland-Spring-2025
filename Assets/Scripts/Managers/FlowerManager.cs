using System;
using UnityEngine;

[RequireComponent(typeof(FlowerShopDialogue))]
public class FlowerManager : MonoBehaviour
{
    private FlowerShopDialogue dialogueManager;

    public enum FlowerGameState
    {
        START_DIALOGUE = 0,
        DETHORN,
        TRIM,
        ARRANGE,
        END_DIALOGUE
    }

    private FlowerGameState currentState = FlowerGameState.START_DIALOGUE;

    [SerializeField]
    private GameObject dethornGame;
    [SerializeField]
    private GameObject trimGame;
    [SerializeField]
    private GameObject arrangeGame;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueManager = GetComponent<FlowerShopDialogue>();
        ChangeState(FlowerGameState.START_DIALOGUE);
    }

    public void ChangeStateInt(int state_num)
    {
        Debug.Log("ALfjdJSFDKS");

        ChangeState((FlowerGameState)state_num);
    }

    public void ChangeState(FlowerGameState state)
    {
        currentState = state;
        switch(currentState)
        {
            case FlowerGameState.START_DIALOGUE:
                dialogueManager.DoIntroDialogue();
                dethornGame.SetActive(false);
                trimGame.SetActive(false);
                arrangeGame.SetActive(false);
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
                break;
            default:
                break;
        }
    }
}
