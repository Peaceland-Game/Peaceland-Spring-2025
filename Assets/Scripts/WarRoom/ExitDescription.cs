using UnityEngine;
using UnityEngine.UI;
using System;

public class ExitDescription : ButtonUtils
{
    [SerializeField]
    public Button viewDescButton;

    [SerializeField]
    //public GameObject scrollView;
    public GameObject panel;

    [SerializeField]
    public Button exitDescButton;

    [SerializeField]
    public Button exitWallView;

    [SerializeField]
    public Button leftArrow;

    [SerializeField]
    public Button rightArrow;

    public event EventHandler OnExitDescriptionClicked;

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

}
