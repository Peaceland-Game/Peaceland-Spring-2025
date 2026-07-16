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
    public GameObject zoomContainer;


    void Start()
    {
        //Debug.Log("Artifact zoom active");
        DisableButton(exitZoom);
    }

    public void ZoomIn()
    {
        //for (int i = 0; i < artifactButtons.Length; i++)
        //{
        //    EnableButton(artifactButtons[i]);
        //}
        Vector2 objectPos = focusPoint.GetComponent<RectTransform>().position;
        //Vector3[] corners = new Vector3[4];
        //focusPoint.GetComponent<RectTransform>().GetWorldCorners(corners);
       // Vector3 world_center = (corners[0] + corners[2]) / 2f;

        //Vector2 local_center = zoomContainer.GetComponent<RectTransform>().InverseTransformPoint(world_center);

       // zoomContainer.GetComponent<RectTransform>().anchoredPosition -= local_center;

        //Debug.Log("Object position: " + local_center);
        //GetComponent<Camera>().GetComponent<Camera>().transform.position = new Vector3(objectPos.x, objectPos.y, -10);
        //GetComponent<Camera>().GetComponent<Camera>().orthographicSize = 1f; // Adjust this value as needed for zoom level
        

        float scale = 3.5f; // Adjust this value as needed for zoom level
        zoomContainer.GetComponent<RectTransform>().localScale = new Vector2(scale, scale);
        zoomContainer.GetComponent<RectTransform>().position = new Vector2(0 - scale * objectPos.x, 0 - scale * objectPos.y);
        

        EnableButton(exitZoom);
        //ToggleButtonArray(true, artifactButtons);
    }



}
