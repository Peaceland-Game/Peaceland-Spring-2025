using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExitZoom : MonoBehaviour
{
    [SerializeField]
    public Camera camera;

    [SerializeField]
    public Button returnToLobbyButton;

    [SerializeField]
    public Button exitZoom;

    [SerializeField]
    public Button viewDescButton;

    public void ExitZoomOnClick()
    {
        Debug.Log("Exiting zoom...");
        camera.transform.position = new Vector3(0, 0, -10); // Reset to original position
        camera.GetComponent<Camera>().orthographicSize = 5f; // Reset to original zoom level
        EnableButton(returnToLobbyButton); // Enable the return to lobby button
        DisableButton(exitZoom); // Disable the exit zoom button
        DisableButton(viewDescButton); // Disable the view description button
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
