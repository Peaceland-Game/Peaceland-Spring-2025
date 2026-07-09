using UnityEngine;
using UnityEngine.UI;

public class ZoomOnObject : ButtonUtils
{
    [SerializeField]
    public GameObject camera;

    [SerializeField]
    public GameObject focusPoint;

    [SerializeField]
    public Button exitZoom;

    [SerializeField]
    public Button[] artifactButtons;




    void Start()
    {
        DisableButton(exitZoom);
    }

    public void ZoomIn()
    {
        Debug.Log("Zooming in on object...");


        Vector2 objectPos = focusPoint.transform.position;
        camera.GetComponent<Camera>().transform.position = new Vector3(objectPos.x, objectPos.y, -10);
        camera.GetComponent<Camera>().orthographicSize = 1f; // Adjust this value as needed for zoom level
        EnableButton(exitZoom);
        ToggleButtonArray(true, artifactButtons);
    }



}
