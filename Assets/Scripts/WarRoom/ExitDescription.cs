using UnityEngine;
using UnityEngine.UI;
using System;

public class ExitDescription : ButtonUtils
{
    [SerializeField]
    public Button viewDescButton = null;

    [SerializeField]
    //public GameObject scrollView;
    public GameObject panel;

    [SerializeField]
    public Button exitDescButton;

    [SerializeField]
    public Button exitWallView = null;

    [SerializeField]
    public Button leftArrow = null;

    [SerializeField]
    public Button rightArrow = null;

    [SerializeField]
    public Button exitZoom = null;


    public event EventHandler OnExitDescriptionClicked; //subscribed by HoverButton.cs to re-enable the hover button when exiting the description

    //[SerializeField]
    //public Button exitZoomButton;

    void Start()
    {

    }

    public void ExitDescriptionOnClick()
    {
        Debug.Log("Exiting description...");
        EnableButton(exitWallView);
        EnableButton(leftArrow);
        EnableButton(rightArrow);
        EnableButton(viewDescButton); // Enable the view description button


        //scrollView.SetActive(false); // Hide the scroll view with the description
        panel.SetActive(false);
        DisableButton(exitDescButton); // Disable the exit description button
        OnExitDescriptionClicked?.Invoke(this, EventArgs.Empty); // Raise the event to notify subscribers
    }

    public void ExitDescriptionForZoom()
    {
        Debug.Log("Exiting description for zoom...");
        panel.SetActive(false);
        DisableButton(exitDescButton);

        if(exitZoom != null)
        {
            EnableButton(exitZoom); // Enable the exit zoom button
        }

        OnExitDescriptionClicked?.Invoke(this, EventArgs.Empty); // Raise the event to notify subscribers
    }

}
