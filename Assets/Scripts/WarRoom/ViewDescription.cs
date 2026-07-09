using UnityEngine;
using UnityEngine.UI;
using System;

public class ViewDescription : ButtonUtils
{
    [SerializeField]
    public Button viewDescButton;

    //[SerializeField]
    //public GameObject scrollView;

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
    public Button seeMore=null;

    public event EventHandler OnViewDescriptionClicked; //subscribed by HoverButton.cs to re-enable the hover button when exiting the description

    //[SerializeField]
    //public string descText="";

    //[SerializeField]
    //public Button exitZoomButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        DisableButton(exitDescButton); // Disable the exit description button at the start
        //scrollView.SetActive(false);
        panel.SetActive(false);
        
    }

    public void ViewDescriptionOnClick()
    {
        Debug.Log("Viewing description...");
        DisableButton(exitWallView);
        DisableButton(leftArrow);
        DisableButton(rightArrow);
        DisableButton(viewDescButton); // Disable the view description button

        //scrollView.SetActive(true); // Show the scroll view with the description
        panel.SetActive(true);
        //scrollView.GetComponentInChildren<Text>().text = descText;
        EnableButton(exitDescButton); // Enable the exit description button
        //DisableButton(exitZoomButton); // Disable the exit zoom button while viewing description
        OnViewDescriptionClicked?.Invoke(this, EventArgs.Empty); // Raise the event to notify subscribers

        if(seeMore != null)
        {
            EnableButton(seeMore);
        }
    }
}
