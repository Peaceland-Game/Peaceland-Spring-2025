using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script that allows an object to be zoomed in on without animation; works for general purposes
/// but was created to work for War Room artifacts
/// </summary>

public class ZoomOnObject : ButtonUtils
{
    [SerializeField]
    public GameObject focusPoint;   //the object itself is used as this focus point

    [SerializeField]
    public GameObject exitZoom;     //button for returning from zoom

    [SerializeField]
    public GameObject zoomContainer;    //the parent (ideally a direct child of Canvas)
                                        //that holds every visible element in the scene


    void Start()
    {
        exitZoom.SetActive(false); // Disable the exit zoom button at the start
    }

    /// <summary>
    /// Zooms in one focusPoint
    /// Future consideration: turn scale into a field for custom zoom levels
    /// </summary>
    public void ZoomIn()
    {
        Vector2 objectPos = focusPoint.GetComponent<RectTransform>().position;
        
        float scale = 3.5f; // Adjust this value as needed for zoom level
        zoomContainer.GetComponent<RectTransform>().localScale = new Vector2(scale, scale);
        zoomContainer.GetComponent<RectTransform>().position = new Vector2(0 - scale * objectPos.x, 0 - scale * objectPos.y);

        if (exitZoom != null) {
            exitZoom.SetActive(true); // Enable the exit zoom button
        }
        

    }



}
