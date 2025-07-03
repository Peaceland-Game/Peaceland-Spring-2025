using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Yarn.Unity;

//this script is being used exclusively for functionality in the Museum Intro scene
public class ScreenTransitioner : MonoBehaviour
{
    //animator
    public Animator transition;

    //bools, used to start the scene and enable/disable animated text
    private bool started;
    private bool textStarted;

    //tap action to progress
    private InputAction tap;

    //text to animate
    public TMPro.TextMeshProUGUI continueText;

    //all iamges in scene
    public UnityEngine.UI.Image newsPaper;
    public UnityEngine.UI.Image whiteFade;
    public UnityEngine.UI.Image museumWide;
    public UnityEngine.UI.Image museumTree;
    public UnityEngine.UI.Image memoryObjectZoomed;
    public UnityEngine.UI.Image museumTreeClose;
    public UnityEngine.UI.Image memoryObjectHanging;
    public SpriteRenderer mira;

    //color for fadeWhite
    private Color color;

    //game manager reference
    private GameManager gm;

    //dialogue runner reference
    public DialogueRunner dr;


    private void Start()
    {
        gm = GameManager.Instance;

        started = false;
        textStarted = false;

        continueText.enabled = false;

        museumTree.enabled = false;
        museumWide.enabled = false;
        memoryObjectZoomed.enabled = false;
        museumTreeClose.enabled = false;
        memoryObjectHanging.enabled = false;

        mira.enabled = false;

        color = whiteFade.color;

        tap = InputSystem.actions.FindAction("Tap");
    }

    // Update is called once per frame
    void Update()
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
            if(tap.IsPressed())
            {
                gm.newsRead = true;

                Debug.Log("NEWS READ TAPPED");

                transition.SetTrigger("NewsRead");

                StartCoroutine(NewsTransition());
            }
        }

        //enable mira when her dialogue starts
        if (dr.CurrentNodeName == "MiraMuseum" && gm.miraIntroDone == false)
        {
            mira.enabled = true;
        }

        //when mira is done
        if (gm.miraIntroDone == true && gm.memoryObjectAcquired == false)
        {
            //so that this code only runs once
            gm.memoryObjectAcquired = true;
            mira.enabled = false;

            StartCoroutine(ClippingTransition());
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
        dr.enabled = false;
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
        yield return new WaitForSeconds(5);
        transition.SetBool("MemoryFadeOut", true);

        //transition scene into the memory intro once screen has gone white
        yield return new WaitForSeconds(4);
        //PUT LEVEL LOAD HERE
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
        gm.introSprawlDone = true;
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
