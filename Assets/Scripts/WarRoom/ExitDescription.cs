using UnityEngine;
using UnityEngine.UI;

public class ExitDescription : ButtonUtils
{
    [SerializeField]
    public Button viewDescButton;

    [SerializeField]
    public GameObject scrollView;

    [SerializeField]
    public Button exitDescButton;

    [SerializeField]
    public Button exitWallView;

    [SerializeField]
    public Button leftArrow;

    [SerializeField]
    public Button rightArrow;

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


        scrollView.SetActive(false); // Hide the scroll view with the description
        DisableButton(exitDescButton); // Disable the exit description button
    }

}
