using UnityEngine;
using UnityEngine.UI;

public class ZoomOnObject : MonoBehaviour
{

    [SerializeField]
    public GameObject scrollView;

    [SerializeField]
    public GameObject camera;

    [SerializeField]
    public GameObject image;

    [SerializeField]
    public Button returnToLobbyButton;

    [SerializeField]
    public Button exitZoom;

    [SerializeField]
    public Button viewDescButton;



    void Start()
    {
        scrollView.SetActive(false);
        DisableButton(viewDescButton); // Disable the view description button at the start
    }

    public void ZoomOnClick()
    {
        Debug.Log("Zooming in on object...");
        DisableButton(returnToLobbyButton); // Disable the return to lobby button
        EnableButton(exitZoom); // Enable the exit zoom button
        EnableButton(viewDescButton); // Enable the view description button


        Vector2 objectPos = image.transform.position;
        camera.transform.position = new Vector3(objectPos.x, objectPos.y, -10);
        camera.GetComponent<Camera>().orthographicSize = 0.6f; // Adjust this value as needed for zoom level
    }

    private void EnableButton(UnityEngine.UI.Button button)
    {
        button.interactable = true;
        button.GetComponent<UnityEngine.UI.Image>().enabled = true;
    }

    private void DisableButton(UnityEngine.UI.Button button)
    {
        button.interactable = false;
        button.GetComponent<UnityEngine.UI.Image>().enabled = false;
    }

}
