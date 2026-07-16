using UnityEngine;
using Unity.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using UnityEditor.Rendering.Universal.ShaderGUI;

public class TownSquareManager : MonoBehaviour
{
    public UnityEngine.UI.Image townSquare;
    public UnityEngine.UI.Image bakeryFront;
    public UnityEngine.UI.Image bakeryInside;
    [SerializeField] BoxCollider2D boxCollider;
    [SerializeField] BoxCollider2D boxCollider2;
    [SerializeField] DialogueRunner dialogueRunner;
    private bool bakeryStart = false;
    private bool fightHappened = false;
    public List<GameObject> npcs = new List<GameObject>();
    public GameObject dalila;
    public GameObject branko;
    public GameObject katarina;

    //Initializes variables and plays the starting dialogue
    void Start()
    {
        dalila.SetActive(false);
        branko.SetActive(false);
        katarina.SetActive(false);
        bakeryFront.enabled = false;
        bakeryInside.enabled = false;
        boxCollider.enabled = false;
        boxCollider2.enabled = false;
        if (!dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue("TownSquareIntro");
        }
    }

    //While the constant repeated use of if statements may seem a bit messy and repetitive, it does help manage the constant event-after-event loop that goes on in this scene after each bit of dialogue
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
            dalila.SetActive(false);
            bakeryInside.enabled = false;
            boxCollider.enabled = false;
            townSquare.enabled = true;
            dialogueRunner.StartDialogue("FightIntro");
            fightHappened = true;
        }
        //if (!dialogueRunner.IsDialogueRunning && fightHappened)
        //{
        //    SceneManager.LoadScene("Home");
        //}
        ToBakery();
        InsideBakery();
    }

    /// <summary>
    /// Triggers dialogue when the bakery is clicked on in the scene
    /// This transitions to the outside of the bakery, changing the background, as well as disabling NPCs and collider for the bakery itself
    /// </summary>
    void ToBakery()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (boxCollider.OverlapPoint(mouseWorld))
            {
                for (int i = 0; i < npcs.Count; i++)
                {
                    npcs[i].SetActive(false);
                }
                townSquare.enabled = false;
                bakeryFront.enabled = true;
                boxCollider.enabled = false;
                bakeryStart = true;
                dialogueRunner.StartDialogue("BakeryFront");
            }
        }
    }

    /// <summary>
    /// Triggers dialogue when the door to the bakery is clicked on
    /// This transitions from the outside of the bakery, to its inside when the entrance is clicked on, as well as turning off the door collider as well
    /// </summary>
    void InsideBakery()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (boxCollider2.OverlapPoint(mouseWorld))
            {
                dalila.SetActive(true);
                bakeryFront.enabled = false;
                bakeryInside.enabled = true;
                boxCollider2.enabled = false;
                dialogueRunner.StartDialogue("BakeryStart");
            }
        }
    }
}
