using UnityEngine;
using UnityEngine.UI;

public class ZoomOnObject : ButtonUtils
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
        camera.GetComponent<Camera>().transform.position = new Vector3(objectPos.x, objectPos.y, -10);
        camera.GetComponent<Camera>().orthographicSize = 100f; // Adjust this value as needed for zoom level
    }

}
