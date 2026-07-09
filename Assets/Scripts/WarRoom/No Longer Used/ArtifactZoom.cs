using UnityEngine;
using UnityEngine.UI;

public class ZoomOnObject : ButtonUtils
{
    //[SerializeField]
    //public GameObject camera;

    [SerializeField]
    public GameObject focusPoint;

    [SerializeField]
    public Button exitZoom;

    [SerializeField]
    public Button[] artifactButtons;

    [SerializeField]
    public GameObject zoomContainer;


    void Start()
    {
        DisableButton(exitZoom);
    }

    public void ZoomIn()
    {
        Debug.Log("Zooming in on object...");


        Vector2 objectPos = focusPoint.GetComponent<RectTransform>().position;
        Debug.Log("Object position: " + objectPos);
        //GetComponent<Camera>().GetComponent<Camera>().transform.position = new Vector3(objectPos.x, objectPos.y, -10);
        //GetComponent<Camera>().GetComponent<Camera>().orthographicSize = 1f; // Adjust this value as needed for zoom level
        zoomContainer.GetComponent<RectTransform>().position = new Vector2(objectPos.x, objectPos.y);
        //zoomContainer.GetComponent<RectTransform>().localScale = new Vector2(2f, 2f); // Adjust this value as needed for zoom level

        EnableButton(exitZoom);
        ToggleButtonArray(true, artifactButtons);
    }



}
