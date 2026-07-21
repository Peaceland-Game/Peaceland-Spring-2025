using UnityEngine;
using UnityEngine.UI;

public class ZoomOnObject : ButtonUtils
{
    //[SerializeField]
    //public GameObject camera;

    [SerializeField]
    public GameObject focusPoint;

    [SerializeField]
    public GameObject exitZoom;

    [SerializeField]
    public GameObject zoomContainer;


    void Start()
    {
        //Debug.Log("Artifact zoom active");
        exitZoom.SetActive(false); // Disable the exit zoom button at the start
    }

    public void ZoomIn()
    {
        Vector2 objectPos = focusPoint.GetComponent<RectTransform>().position;
        
        float scale = 3.5f; // Adjust this value as needed for zoom level
        zoomContainer.GetComponent<RectTransform>().localScale = new Vector2(scale, scale);
        zoomContainer.GetComponent<RectTransform>().position = new Vector2(0 - scale * objectPos.x, 0 - scale * objectPos.y);

        exitZoom.SetActive(true); // Enable the exit zoom button

    }



}
