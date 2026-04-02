using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField]
    public TMPro.TextMeshProUGUI continueText;  // Animated "Continue" Text

    // Images for the scene
    public UnityEngine.UI.Image newsPaper;
    public UnityEngine.UI.Image whiteFade;
    public UnityEngine.UI.Image museumWide;
    public UnityEngine.UI.Image museumTree;
    public UnityEngine.UI.Image memoryObjectZoomed;
    public UnityEngine.UI.Image museumTreeClose;
    public UnityEngine.UI.Image memoryObjectHanging;

    [SerializeField]
    private Color fadeColor;        // Color for fadeout effect

    public LevelLoader LL;          // Level Loader reference

    // Methods:

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set up variables
        //    GM = GameManager.Instance;
        GM = FindFirstObjectByType<GameManager>();
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
        fadeColor = whiteFade.color;
        tap = InputSystem.actions.FindAction("Tap");

        // Hide Background Screens
        started = false;
        textStarted = false;

        continueText.enabled = false;

        museumTree.enabled = false;
        museumWide.enabled = false;
        memoryObjectZoomed.enabled = false;
        museumTreeClose.enabled = false;
        memoryObjectHanging.enabled = false;

        // Hide character portraits
        MainCharPortrait.SetActive(false);
        SecondCharPortrait.SetActive(false);

        // Testing calls. Will not work in the actual demo
        //    NextMinigame();
        //    NextOrder();

        // Jump ahead on dialoge if returning from the memory
        if (GM.seenRJMemory)
        {
            currentMinigame = afterMemStart - 1;    // NextMinigame increments the count
            NextOrder();
            NextMinigame();

            museumTree.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Run coroutines if the memory hasn't been played yet
        if (!GM.seenRJMemory)
        {
            //if the news (first screen) hasn't been read yet
            if (transition.GetBool("NewsRead") == false)
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
                if (tap.IsPressed())
                {
                    GM.newsRead = true;

                    Debug.Log("NEWS READ TAPPED");

                    transition.SetTrigger("NewsRead");

                    StartCoroutine(NewsTransition());
                }
            }

            // Once the intro is done, start the dialogue
            if (GM.introSprawlDone && !GM.marcStart)
            {
                GM.marcStart = true;
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
            StartCoroutine(ClippingTransition());
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
            StartCoroutine(ClippingTransition());
            LL.LoadLevelByBuildIndex(4);
        }

        else
        {
            Debug.Log($"Curent mingame index = {currentMinigame}. Only 1 or 2 change the scene");
        }
    }

    //functionality for everything post-dialogue
    IEnumerator ClippingTransition()
    {
        //begin fade into white right after mira conversation
        Debug.Log("Begin Clipping Transition");
        transition.SetBool("DialogueEnd", true);

        //wait until screen is white, then start fade into news clipping. Enable/disable appropriate assets
        yield return new WaitForSeconds(4);
        transition.SetBool("ClippingStart", true);
        museumTree.enabled = false;
        dialogueRunner.enabled = false;
        memoryObjectZoomed.enabled = true;

        //wait until fade is done and player has looks at clipping for a few seconds
        yield return new WaitForSeconds(7);
        transition.SetBool("ClippingDone", true);

        //wait for screen to go white again, enable/disable appropriate assets
        yield return new WaitForSeconds(4);
        transition.SetBool("ShowCloseTree", true);
        memoryObjectZoomed.enabled = false;
        museumTreeClose.enabled = true;
        memoryObjectHanging.enabled = true;

        //wait until player has looked at memory tree with the clipping on it, begin fade into the memory
        yield return new WaitForSeconds(6);
        transition.SetBool("MemoryFadeOut", true);

        //transition scene into the memory intro once screen has gone white
        yield return new WaitForSeconds(4);
        //PUT LEVEL LOAD HERE

        LL.LoadNextLevel();
    }

    //whole transition sequence for newspaper and auto advance screens
    IEnumerator NewsTransition()
    {
        //screen fade, disable news and text
        Debug.Log("Begin News Transition");
        yield return new WaitForSeconds(4);
        newsPaper.enabled = false;
        transition.SetBool("TextEnd", true);
        continueText.enabled = false;

        //show outside of musuem
        museumWide.enabled = true;
        transition.SetBool("MuseumOut", true);
        yield return new WaitForSeconds(7);

        //fade into white
        transition.SetBool("MuseumOutDone", true);
        yield return new WaitForSeconds(4);

        //disable museum outside, enable museum inside, set var to true, begin white fadout to museum inside
        museumWide.enabled = false;
        museumTree.enabled = true;
        GM.introSprawlDone = true;
        transition.SetBool("IntroFadeout", true);

    }

    //start animating text
    IEnumerator TextStart()
    {
        Debug.Log("Show Continue Text");
        textStarted = true;
        yield return new WaitForSeconds(5);
        transition.SetBool("TextStart", true);
        continueText.enabled = true;
    }

    //start scene and wait 3 seconds
    IEnumerator Wait()
    {
        started = true;
        Debug.Log("Waiting");
        yield return new WaitForSeconds(3);
    }
}
