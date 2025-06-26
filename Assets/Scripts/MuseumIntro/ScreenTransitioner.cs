using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEditor.Build;

public class ScreenTransitioner : MonoBehaviour
{
    public Animator transition;

    private bool newsRead; //this should be put into GameManager
    private bool started;
    private bool textStarted;
    private bool introSprawlDone;

    private InputAction tap;
    public TMPro.TextMeshProUGUI continueText;

    public UnityEngine.UI.Image newsPaper;
    public UnityEngine.UI.Image whiteFade;
    public UnityEngine.UI.Image museumWide;
    public UnityEngine.UI.Image museumTree;

    private Color color;


    private void Start()
    {
        newsRead = false;
        started = false;
        textStarted = false;
        introSprawlDone = false;

        continueText.enabled = false;

        museumTree.enabled = false;
        museumWide.enabled = false;

        color = whiteFade.color;

        tap = InputSystem.actions.FindAction("Tap");
    }

    // Update is called once per frame
    void Update()
    {
        if (transition.GetBool("NewsRead") == false)
        {
            if (started == false)
            {
                StartCoroutine(Wait());
            }

            if (textStarted == false)
            {
                StartCoroutine(TextStart());
            }

            if(tap.IsPressed())
            {
                newsRead = true;

                Debug.Log("NEWS READ TAPPED");

                transition.SetTrigger("NewsRead");

                StartCoroutine(NewsTransition());
            }
        }

        if (introSprawlDone == true)
        {
            //rest of functionality
        }
        
    }


    IEnumerator NewsTransition()
    {
        Debug.Log("Begin News Transition");
        yield return new WaitForSeconds(4);
        newsPaper.enabled = false;
        transition.SetBool("TextEnd", true);
        continueText.enabled = false;

        museumWide.enabled = true;
        transition.SetBool("MuseumOut", true);
        yield return new WaitForSeconds(7);

        transition.SetBool("MuseumOutDone", true);
        yield return new WaitForSeconds(4);

        museumWide.enabled = false;
        museumTree.enabled = true;
        introSprawlDone = true;
        transition.SetBool("IntroFadeout", true);

    }

    IEnumerator TextStart()
    {
        Debug.Log("Show Continue Text");
        textStarted = true;
        yield return new WaitForSeconds(5);
        transition.SetBool("TextStart", true);
        continueText.enabled = true;
    }

    IEnumerator Wait()
    {
        started = true;
        Debug.Log("Waiting");
        yield return new WaitForSeconds(3);
    }
}
