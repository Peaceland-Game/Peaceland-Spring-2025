using UnityEngine;
using UnityEngine.UI;

public class ZoomOnObject : MonoBehaviour
{
    [SerializeField]
    public Camera camera;

    [SerializeField]
    public GameObject image;

    [SerializeField]
    public Button exitZoom;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //exitZoom.enabled = false; // Disable the exit zoom button at the start
        //exitZoom.image.enabled = false; // Hide the button image
        //exitZoom.text.enabled = false; // Hide the button image
        Debug.Log("Camera found: " + (camera != null));
    }

    public void ZoomOnClick()
    {
        Debug.Log("Zooming in on object...");
        Vector2 objectPos = image.transform.position;
        camera.transform.position = new Vector3(objectPos.x, objectPos.y, -10);
        camera.GetComponent<Camera>().orthographicSize = 0.6f; // Adjust this value as needed for zoom level
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
