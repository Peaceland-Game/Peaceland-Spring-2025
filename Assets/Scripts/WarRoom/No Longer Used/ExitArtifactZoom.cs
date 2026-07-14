using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class ExitZoom : ButtonUtils
{
    //[SerializeField]
    //public Button returnToLobbyButton;

    [SerializeField]
    public Button exitZoom;

    [SerializeField]
    public Button viewDescButton;

    //[SerializeField]
    //public Button exitWallViewButton;

    [SerializeField]
    public GameObject zoomContainer;



    void Start()
    {
        DisableButton(viewDescButton); // Disable the view description button at the start
        DisableButton(exitZoom);
    }
    public void ExitZoomOnClick()
    {
        Debug.Log("Exiting zoom...");
        zoomContainer.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        zoomContainer.GetComponent<RectTransform>().position = new Vector2(0, 0);
        DisableButton(exitZoom); // Disable the exit zoom button

        //EnableButton(returnToLobbyButton); // Enable the return to lobby button
        //EnableButton(exitWallViewButton);
        //EnableButton(exitDescButton); // Enable the exit description button
        //panel.SetActive(true); // Enable the description panel
        //DisableButton(exitZoom); // Disable the exit zoom button
        //DisableButton(viewDescButton); // Disable the view description button

        //OnViewDescriptionClicked?.Invoke(this, EventArgs.Empty); // Raise the event to notify subscribers

        //if (seeMore != null)
        //{
        //    EnableButton(seeMore);
        //}
    }
}
