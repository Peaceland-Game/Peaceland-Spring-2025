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
    public Button exitZoomButton;

    void Start()
    {
        DisableButton(exitDescButton); // Disable the exit description button at the start
    }

    public void ExitDescriptionOnClick()
    {
        Debug.Log("Exiting description...");
        scrollView.SetActive(false); // Hide the description when the exit button is clicked
        EnableButton(viewDescButton); // Enable the view description button
        DisableButton(exitDescButton); // Disable the exit description button
        EnableButton(exitZoomButton); // Enable the exit zoom button when exiting description
    }

}
