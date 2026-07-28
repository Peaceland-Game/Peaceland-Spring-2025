using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Enables description view for an artifact, by enabling and disabling specific buttons and panels when
/// </summary>
public class ViewDescription : ButtonUtils
{
    [SerializeField]
    public Button viewDescButton;   

    [SerializeField]
    public GameObject panel;

    [SerializeField]
    public Button exitDescButton;
    
    [SerializeField]
    public Button exitWallView;

    [SerializeField]
    public Button leftArrow;

    [SerializeField]
    public Button rightArrow;

    [SerializeField]
    public Button seeMore=null;     //for artifacts that have additional content beyond just the simple description

    [SerializeField]
    public Button exitZoom = null;

    public event EventHandler OnViewDescriptionClicked; //subscribed by HoverButton.cs to re-enable the hover button when exiting the description


    void Start()
    {
        DisableButton(exitDescButton); // Disable the exit description button at the start
        panel.SetActive(false);
        
    }

    public void ViewDescriptionOnClick()
    {
        //disables wall view buttons
        DisableButton(exitWallView);
        DisableButton(leftArrow);
        DisableButton(rightArrow);
        viewDescButton.interactable = false;
        if (exitZoom != null)
        {
            DisableButton(exitZoom);
        }

        //enables desc view buttons
        panel.SetActive(true);
        EnableButton(exitDescButton);
        if (seeMore != null)
        {
            EnableButton(seeMore);
        }

        OnViewDescriptionClicked?.Invoke(this, EventArgs.Empty); // Raise the event to notify subscribers
    }
}
