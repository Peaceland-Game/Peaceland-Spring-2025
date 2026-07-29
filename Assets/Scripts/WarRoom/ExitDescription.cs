using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// This class provides functionality to enable and disable specific buttons and panels when exiting artifact
/// description view. It also raises an event to notify subscribers when the exit description action is
/// performed.
public class ExitDescription : ButtonUtils
{
    [SerializeField]
    public Button viewDescButton = null;    //the button that enters description views

    [SerializeField]
    public GameObject panel;                //the panel that serves as a container and background for the description

    [SerializeField]
    public Button exitDescButton;           //button that exits description view

    [SerializeField]
    public Button exitWallView = null;      //button that exits wall view (individual walls)

    [SerializeField]
    public Button leftArrow = null;         //button that moves one wall to the left

    [SerializeField]
    public Button rightArrow = null;        //button that moves one wall to the right

    [SerializeField]
    public Button exitZoom = null;          //button that exits zoomed view

    public event EventHandler OnExitDescriptionClicked; //subscribed by HoverButton.cs to re-enable the hover button when exiting the description

    void Start()
    {

    }

    /// <summary>
    /// closes panel and re-enables wall view buttons
    /// </summary>
    public void ExitDescriptionOnClick()
    {
        //enables wall view buttons 
        EnableButton(exitWallView);
        EnableButton(leftArrow);
        EnableButton(rightArrow);
        EnableButton(viewDescButton); // Enable the view description button


        //disables the description view buttons
        panel.SetActive(false);
        DisableButton(exitDescButton); // Disable the exit description button
        OnExitDescriptionClicked?.Invoke(this, EventArgs.Empty); // Raise the event to notify subscribers
    }

    public void ExitDescriptionForZoom()
    {
        panel.SetActive(false);
        DisableButton(exitDescButton);

        if(exitZoom != null)
        {
            EnableButton(exitZoom); // Enable the exit zoom button
        }

        OnExitDescriptionClicked?.Invoke(this, EventArgs.Empty); // Raise the event to notify subscribers
    }

}
