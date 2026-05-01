using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manager script for the Spring 2026 demo. Created from MuseumIntroManager.cs and ScreenTransitioner.cs.
/// Likely not useful for future use, as the backend of the game will change.
/// </summary>
public class Demo_RJMuseumIntro : GenericMemManager
{
    // Fields
    // List of minigames, SceneCharacters, and DialogueRunner inherited from parent.
    [SerializeField]
    private int afterMemStart;      // Minigame index to start at after finishing the memory
    [SerializeField]
    private GameObject MainCharPortrait;
    [SerializeField]
    private GameObject SecondCharPortrait;

    // From MuseumIntroManager.cs:
    private GameManager GM;     // Reference to the Game Manager

    // From ScreenTransitioner.cs
    [SerializeField]
    private Animator transition;     // Used to control animations like the screen fading

    private bool started;           // Bool to start scene
    private bool textStarted;       // Bool to enable "Press to Continue" text

    private InputAction tap;        // Checks for user input via tap or click

    //[SerializeField]
    //public TMPro.TextMeshProUGUI continueText;  // Animated "Continue" Text

    [SerializeField]
    public Button continueButton;

    // Images for the scene
    public UnityEngine.UI.Image newsPaper;
//    public UnityEngine.UI.Image whiteFade;
    public UnityEngine.UI.Image museumWide;
    public SpriteRenderer marcBack;
    public UnityEngine.UI.Image museumTree;
    public SpriteRenderer treePlaque;
//    public UnityEngine.UI.Image memoryObjectZoomed;
//    public UnityEngine.UI.Image museumTreeClose;
//    public UnityEngine.UI.Image memoryObjectHanging;

    //[SerializeField]
    //private Color fadeColor;        // Color for fadeout effect

    // Methods:

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set up variables
        GM = FindFirstObjectByType<GameManager>();
        LL = FindFirstObjectByType<LevelLoader>();
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
    //    fadeColor = whiteFade.color;
        tap = InputSystem.actions.FindAction("Tap");

        // Hide Background Screens
        started = false;
        textStarted = false;

        //continueText.enabled = false;

        continueButton.interactable = false;
        continueButton.GetComponent<Image>().enabled = false;

        museumTree.enabled = false;
        treePlaque.enabled = false;
        museumWide.enabled = false;
        marcBack.enabled = false;
    //    memoryObjectZoomed.enabled = false;
    //    museumTreeClose.enabled = false;
    //    memoryObjectHanging.enabled = false;

        // Hide character portraits
        MainCharPortrait.SetActive(false);
        SecondCharPortrait.SetActive(false);

        // Jump ahead on dialoge if returning from the memory
        if (GM.seenRJMemory)
        {
            newsPaper.enabled = false;
            currentMinigame = afterMemStart - 1;    // NextMinigame increments the count
            NextOrder();
            NextMinigame();

            museumTree.enabled = true;
            treePlaque.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Run coroutines if the memory hasn't been played yet
        if (!GM.seenRJMemory)
        {
            //if the news (first screen) hasn't been read yet
            if (transition.GetBool("NewsRead") == false || transition.GetBool("MuseumClicked") == false)
            {
                //start the sequence
                if (started == false)
                {
                    StartCoroutine(Wait());
                }

                //animated text for "press to continue"
                if (textStarted == false)
                {
                    StartCoroutine(TextStart());
                }

                //when pressed, carry out the rest of the intro sequence
                //if (tap.IsPressed() && !GM.newsRead)
                //{
                //    GM.newsRead = true;

                //    Debug.Log("NEWS READ TAPPED");

                //    StartCoroutine(NewsTransition());
                //}

                //if (tap.IsPressed() && museumWide.enabled == true)
                //{
                //    Debug.Log("MUSEUM TAPPED");
                //    StartCoroutine(MuseumTransition());
                //}
            }

            // Once the intro is done, start the dialogue
            if (GM.introSprawlDone && !GM.marcStart)
            {
                GM.marcStart = true;
                StartCoroutine(Wait());
                NextOrder();
                NextMinigame();
            }

            // This is now handled in the Yarn Script
                //enable mira when her dialogue starts
                //if (dialogueRunner.CurrentNodeName == "MiraMuseum" && GM.miraIntroDone == false)
                //{
                //    MainCharPortrait.SetActive(true);
                //    SecondCharPortrait.SetActive(true);
                //}

            // ClippingTransition is now called in NextMinigame
                ////when mira is done
                //if (GM.miraIntroDone == true && GM.memoryObjectAcquired == false)
                //{
                //    //so that this code only runs once
                //    GM.memoryObjectAcquired = true;
                ////    mira.enabled = false;

                //    StartCoroutine(ClippingTransition());
                //}
        }
    }

    public void Continue()
    {
        if (!GM.newsRead)
        {
            GM.newsRead = true;

            Debug.Log("NEWS READ TAPPED");

            StartCoroutine(NewsTransition());
        }

        if (museumWide.enabled == true)
        {
            Debug.Log("MUSEUM TAPPED");
            StartCoroutine(MuseumTransition());
        }
    }

    /// <summary>
    /// Override of NextMinigame that moves the player between scenes
    /// </summary>
    public override void NextMinigame()
    {
        Debug.Log("Current Minigame " + currentMinigame);

        // Stope the current mingame, if there is one.
        if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();

        // Increment minigame counter
        currentMinigame++;

        // If no dialouge yet, start the dialogue
        if (currentMinigame == 0)
        {
            minigames[currentMinigame].StartMinigame();
        }

        // If after the initial dialogue, jump into the memory sequence.
        else if (currentMinigame == 1 && !GM.seenRJMemory)
        {
            LL.LoadNextLevel();
        }

        // If returning from the memory, run the dialogue
        else if (currentMinigame == 1 && GM.seenRJMemory)
        {
            minigames[currentMinigame].StartMinigame();
        }

        // If after the ending dialogue, go to the demo end screen.
        else if (currentMinigame == 2)
        {
            LL.LoadLevelByBuildIndex(4);
        }

        else
        {
            Debug.Log($"Curent mingame index = {currentMinigame}. Only 1 or 2 change the scene");
        }
    }

    //whole transition sequence for newspaper and auto advance screens
    IEnumerator NewsTransition()
    {
        //screen fade, disable news and text
        Debug.Log("Begin News Transition");
        // Fade in
        GameManager.Instance.CurrentMemoryManager.LevelLoader.BlackFadeAnimation(true);
        yield return new WaitForSeconds(0f); //orig 2f

        //Hide newspaper, show outside of musuem
        newsPaper.enabled = false;
        museumWide.enabled = true;
        marcBack.enabled = true;
        Debug.Log("Museum Enabled: " + museumWide.enabled);

        // Fade out
        GameManager.Instance.CurrentMemoryManager.LevelLoader.BlackFadeAnimation(false);
    }

    // Transitions from outside the museum to the memory tree
    IEnumerator MuseumTransition()
    {
        continueButton.interactable = false;
        continueButton.GetComponent<Image>().enabled = false;
        Debug.Log("Start Museum Transition");

        //fade into white
        GameManager.Instance.CurrentMemoryManager.LevelLoader.BlackFadeAnimation(true);
        yield return new WaitForSeconds(0f);
        GameManager.Instance.CurrentMemoryManager.LevelLoader.BlackFadeAnimation(false);
        //continueText.enabled = false;
        

        //disable museum outside, enable museum inside, set var to true, begin black fadout to museum inside
        museumWide.enabled = false;
        marcBack.enabled = false;
        museumTree.enabled = true;
        treePlaque.enabled = true;
        GM.introSprawlDone = true;
        GameManager.Instance.CurrentMemoryManager.LevelLoader.BlackFadeAnimation(false);
    }

    //start animating text
    IEnumerator TextStart()
    {
        Debug.Log("Show Continue Text");
        textStarted = true;
        yield return new WaitForSeconds(0f);
        transition.SetBool("TextStart", true);
        //continueText.enabled = true;
        continueButton.interactable = true;
        continueButton.GetComponent<Image>().enabled = true;
    }

    //start scene and wait 3 seconds
    IEnumerator Wait()
    {
        started = true;
        Debug.Log("Waiting");
        yield return new WaitForSeconds(0f);
    }
}
