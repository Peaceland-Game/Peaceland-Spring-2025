using UnityEngine;
using Unity.UI;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class TownSquareManager : MonoBehaviour
{
    public UnityEngine.UI.Image townSquare;
    public UnityEngine.UI.Image bakeryFront;
    public UnityEngine.UI.Image bakeryInside;
    [SerializeField] BoxCollider2D boxCollider;
    [SerializeField] BoxCollider2D boxCollider2;
    [SerializeField] DialogueRunner dialogueRunner;
    private bool bakeryStart = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bakeryFront.enabled = false;
        bakeryInside.enabled = false;
        boxCollider.enabled = false;
        boxCollider2.enabled = false;
        if (!dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue("TownSquareIntro");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!dialogueRunner.IsDialogueRunning && !bakeryStart)
        {
            boxCollider.enabled = true;
        }
        if (!dialogueRunner.IsDialogueRunning && bakeryFront.enabled)
        {
            boxCollider2.enabled = true;
        }
        if (!dialogueRunner.IsDialogueRunning && bakeryInside.enabled)
        {
            bakeryInside.enabled = false;
            boxCollider.enabled = false;
            townSquare.enabled = true;
        }
        ToBakery();
        InsideBakery();
    }

    void ToBakery()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (boxCollider.OverlapPoint(mouseWorld))
            {
                townSquare.enabled = false;
                bakeryFront.enabled = true;
                boxCollider.enabled = false;
                bakeryStart = true;
                dialogueRunner.StartDialogue("BakeryFront");
            }
        }
    }

    void InsideBakery()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (boxCollider2.OverlapPoint(mouseWorld))
            {
                bakeryFront.enabled = false;
                bakeryInside.enabled = true;
                boxCollider2.enabled = false;
                dialogueRunner.StartDialogue("BakeryStart");
            }
        }
    }
}
