using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exits zoomed in state, returning to normal/standard view;
/// </summary>
public class ExitZoom : ButtonUtils
{
    [SerializeField]
    public GameObject exitZoom;     //the button to click on to exit zoomed state

    [SerializeField]
    public GameObject zoomContainer;    //Container holding every visible/interactable
                                        //object in the scene

    void Start()
    {

    }

    public void ExitZoomOnClick()
    {
        //reset zoom
        zoomContainer.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        zoomContainer.GetComponent<RectTransform>().position = new Vector2(0, 0);

        exitZoom.SetActive(false); // Disable the exit zoom button
    }
}
