using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZoomOnObject : MonoBehaviour
{
    [SerializeField]
    public Camera camera;

    [SerializeField]
    public GameObject image;

    [SerializeField]
    public Button returnToLobbyButton;

    [SerializeField]
    public Button exitZoom;

    public void ZoomOnClick()
    {
        Debug.Log("Zooming in on object...");
        DisableButton(returnToLobbyButton); // Disable the return to lobby button
        EnableButton(exitZoom); // Enable the exit zoom button


        Vector2 objectPos = image.transform.position;
        camera.transform.position = new Vector3(objectPos.x, objectPos.y, -10);
        camera.GetComponent<Camera>().orthographicSize = 0.6f; // Adjust this value as needed for zoom level
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
