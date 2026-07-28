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

    [SerializeField]
    public Button continueButton;

    [SerializeField]
    public Button warRoomEntrance;

    [SerializeField]
    public Button startDialogueButton; //dummy button to start dialogue (replace with npc interaction later)

    [SerializeField]
    public Button endDemoButton;

    // Images for the scene
    public UnityEngine.UI.Image newsPaper;
    public UnityEngine.UI.Image museumWide;
    public SpriteRenderer marcBack;
    public UnityEngine.UI.Image museumTree;
    public SpriteRenderer treePlaque;

    // Build indices
    private int DemoEndBuildIndex = 4;
    private int WarRoomBuildIndex = 6;

    // Methods:

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set up variables
        GM = FindFirstObjectByType<GameManager>();
        LL = FindFirstObjectByType<LevelLoader>();
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
        tap = InputSystem.actions.FindAction("Tap");

        if (GM.allMuseumDialogueComplete)
        {
            currentMinigame = 2;
        }

        if (GM.introSprawlDone && !GM.seenRJMemory)
        {
            newsPaper.enabled = false;
            MainCharPortrait.SetActive(false);
            SecondCharPortrait.SetActive(false);
            DisableContinueButton();
            StartCoroutine(MuseumTransition());
            return;
        }

        Debug.Log("demo start");

        // Hide Background Screens
        started = false;
        textStarted = false;
        DisableContinueButton();

        warRoomEntrance.interactable = false;
        startDialogueButton.interactable = false;
        startDialogueButton.GetComponent<Image>().enabled = false;
        DisableDemoEndButton();

        museumTree.enabled = false;
        treePlaque.enabled = false;
        museumWide.enabled = false;
        marcBack.enabled = false;

        // Hide character portraits
        MainCharPortrait.SetActive(false);
        SecondCharPortrait.SetActive(false);

        // Jump ahead on dialog if returning from the memory
        if (GM.seenRJMemory)
        {
            if (!GM.allMuseumDialogueComplete)
            {
                Debug.Log("Returning from memory, skipping to museum tree");
                
                currentMinigame = afterMemStart - 1;    // NextMinigame increments the count
                NextOrder();
                NextMinigame();
                DisableDemoEndButton();
            }
            else
            {
                endDemoButton.interactable = true;
                endDemoButton.GetComponent<Image>().enabled = true;
            }


            newsPaper.enabled = false;
            museumTree.enabled = true;
            treePlaque.enabled = true;
            warRoomEntrance.interactable = true;
            DisableContinueButton();
            
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
            }

            if (transition.GetBool("MuseumClicked") == true)
            {
                DisableContinueButton();
                DisableDemoEndButton();
            }

            //enable endDemoButton after all dialogue is complete
            if (currentMinigame == 2 || GM.allMuseumDialogueComplete == true)
            {
                GM.allMuseumDialogueComplete = true;
                endDemoButton.interactable = true;
                endDemoButton.GetComponent<Image>().enabled = true;
            }

            // Once the intro is done, start the dialogue
            //if (GM.introSprawlDone && !GM.marcStart)
            //{
            //    StartDialogue();
            //}
        }
    }

    private void DisableContinueButton()
    {
        continueButton.interactable = false;
        continueButton.GetComponent<Image>().enabled = false;
    }

    private void DisableDemoEndButton()
    {
        endDemoButton.interactable = false;
        endDemoButton.GetComponent<Image>().enabled = false;
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
            DisableContinueButton();
            DisableDemoEndButton();
            StartCoroutine(MuseumTransition());
        }
    }

    public void StartDialogue()
    {
        warRoomEntrance.interactable = false;
        
        GM.marcStart = true;
        StartCoroutine(Wait());
        NextOrder();
        NextMinigame();
        startDialogueButton.interactable = false;
        startDialogueButton.GetComponent<Image>().enabled = false;
    }

    /// <summary>
    /// Override of NextMinigame that moves the player between scenes
    /// </summary>
    public override void NextMinigame()
    {
        if (GM.allMuseumDialogueComplete)
        {
            currentMinigame = 2;
            Debug.Log("All dialogue is complete. currentMinigame clamped to 2.");
            return;
        }

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
        else if (currentMinigame == 1 && GM.seenRJMemory && !GM.allMuseumDialogueComplete)
        {
            warRoomEntrance.interactable = false;
            minigames[currentMinigame].StartMinigame();
        }

        // If after the ending dialogue, enable the button that transitions to 
        // demo end screen and make warRoomEntrance interactable again
        else if (currentMinigame == 2)
        {
            GM.allMuseumDialogueComplete = true;
            warRoomEntrance.interactable = true;
            endDemoButton.interactable = true;
            endDemoButton.GetComponent<Image>().enabled = true;
        }

        else
        {
            Debug.Log($"Curent mingame index = {currentMinigame}. Only 1 or 2 change the scene");
        }
    }

    public void EndDemo()
    {
        LL.LoadLevelByBuildIndex(DemoEndBuildIndex);
    }

    public void EnterWarRoom()
    {
        Debug.Log("Entering War Room");
        LL.LoadLevelByBuildIndex(WarRoomBuildIndex);
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
        DisableContinueButton();
        DisableDemoEndButton();
        warRoomEntrance.interactable = true;
        Debug.Log("Start Museum Transition");

        startDialogueButton.interactable = true;
        startDialogueButton.GetComponent<Image>().enabled = true;
       

        //fade into white
        GameManager.Instance.CurrentMemoryManager.LevelLoader.BlackFadeAnimation(true);
        yield return new WaitForSeconds(0f);
        GameManager.Instance.CurrentMemoryManager.LevelLoader.BlackFadeAnimation(false);
        

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
        if (!GM.introSprawlDone)
        {
            Debug.Log("Show Continue Text");
            textStarted = true;
            yield return new WaitForSeconds(0f);
            transition.SetBool("TextStart", true);
            continueButton.interactable = true;
            continueButton.GetComponent<Image>().enabled = true;
        }
        
    }

    //start scene and wait 3 seconds
    IEnumerator Wait()
    {
        started = true;
        Debug.Log("Waiting");
        yield return new WaitForSeconds(0f);
    }
}
