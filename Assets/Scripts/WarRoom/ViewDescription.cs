using UnityEngine;
using UnityEngine.UI;

public class ViewDescription : MonoBehaviour
{
    [SerializeField]
    public Button viewDescButton;

    [SerializeField]
    public GameObject scrollView;

    [SerializeField]
    public Button exitDescButton;

    [SerializeField]
    public Button exitZoomButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        DisableButton(exitDescButton); // Disable the exit description button at the start
    }

    public void ViewDescriptionOnClick()
    {
        Debug.Log("Viewing description...");
        DisableButton(viewDescButton); // Disable the view description button
        scrollView.SetActive(true); // Show the scroll view with the description
        EnableButton(exitDescButton); // Enable the exit description button
        DisableButton(exitZoomButton); // Disable the exit zoom button while viewing description
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
