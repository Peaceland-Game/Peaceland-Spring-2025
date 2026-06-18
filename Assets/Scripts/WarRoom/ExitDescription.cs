using UnityEngine;
using UnityEngine.UI;

public class ExitDescription : MonoBehaviour
{
    [SerializeField]
    public Button viewDescButton;

    [SerializeField]
    public GameObject scrollView;

    [SerializeField]
    public Button exitDescButton;

    [SerializeField]
    public Button exitZoomButton;


    public void ExitDescriptionOnClick()
    {
        Debug.Log("Exiting description...");
        scrollView.SetActive(false); // Hide the description when the exit button is clicked
        EnableButton(viewDescButton); // Enable the view description button
        DisableButton(exitDescButton); // Disable the exit description button
        EnableButton(exitZoomButton); // Enable the exit zoom button when exiting description
    }

    private void EnableButton(Button button)
    {
        button.interactable = true;
        button.GetComponent<Image>().enabled = true;
    }

    private void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().enabled = false;
    }
}
