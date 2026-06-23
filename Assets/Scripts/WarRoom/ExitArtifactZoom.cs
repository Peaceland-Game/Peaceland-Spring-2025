using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExitZoom : ButtonUtils
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
        camera.GetComponent<Camera>().orthographicSize = 540f; // Reset to original zoom level
        EnableButton(returnToLobbyButton); // Enable the return to lobby button
        DisableButton(exitZoom); // Disable the exit zoom button
        DisableButton(viewDescButton); // Disable the view description button
    }
}
