using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class ExitZoom : ButtonUtils
{
    //[SerializeField]
    //public Button returnToLobbyButton;

    [SerializeField]
    public GameObject exitZoom;

    [SerializeField]
    public Button viewDescButton;

    //[SerializeField]
    //public Button exitWallViewButton;

    [SerializeField]
    public GameObject zoomContainer;



    void Start()
    {
    }
    public void ExitZoomOnClick()
    {
        //Debug.Log("Exiting zoom...");
        zoomContainer.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        zoomContainer.GetComponent<RectTransform>().position = new Vector2(0, 0);
        exitZoom.SetActive(false); // Disable the exit zoom button
    }
}
