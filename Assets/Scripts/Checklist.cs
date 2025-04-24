using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//IMPORTANT
//The checklist is no longer being used, but is being kept here in case it or any of the code here is used in the future
//Also being kept for documentation purposes

public class Checklist : MonoBehaviour
{
    /// <summary>
    /// The x-coordinate where the checklist should start
    /// </summary>
    private int checklistStartPoint;

    /// <summary>
    /// The x-coordinate for where the checklist should stop moving
    /// </summary>
    private int checklistStopPoint = -690;

    /// <summary>
    /// Controls whether the checklist can move or not
    /// </summary>
    private bool checklistCanMove = false;

    /// <summary>
    /// Bool to keep track of whether the checklist is moving or not
    /// </summary>
    private bool checklistIsMoving = false;

    /// <summary>
    /// Bool to check whether the checklist has finished moving or not
    /// </summary>
    private bool checklistReachedEnd = false;

    /// <summary>
    /// Holds the text from the checklist
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI checklistText;

    /// <summary>
    /// Public reference to the checklist text
    /// </summary>
    public TextMeshProUGUI ChecklistText
    {
        get { return checklistText; }
        set { checklistText = value; }
    }

    /// <summary>
    /// The RectTransform for the checklist object
    /// </summary>
    private RectTransform rectTransform;

    /// <summary>
    /// Holds data related to clicking/interacting with the game
    /// </summary>
    private PointerEventData clickData;
    /// <summary>
    /// Holds a list of raycast results obtained from elements the user clicked on
    /// </summary>
    private List<RaycastResult> clickResults;

    /// <summary>
    /// Raycaster element to help save clickData to clickResults
    /// </summary>
    private GraphicRaycaster graphicRaycaster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Save the rect transform on the checklist to a var, so the position can be used for the starting point for the checklist
        rectTransform = GetComponent<RectTransform>();
        checklistStartPoint = (int)rectTransform.anchoredPosition.x;

        //Instantiate the clicking vars
        clickData = new PointerEventData(EventSystem.current);
        clickResults = new List<RaycastResult>();
        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        //If checklistCanMove is true, continue
        if (checklistCanMove)
        {
            checklistIsMoving = true;

            //If the checklist is hidden, move it to the right
            if (rectTransform.anchoredPosition.x < checklistStopPoint && !checklistReachedEnd)
            {
                rectTransform.anchoredPosition += new Vector2(20, 0);

                //If the checklist has reached it's stopping point, stop it from moving
                if (rectTransform.anchoredPosition.x >= checklistStopPoint)
                {
                    rectTransform.anchoredPosition.Set(checklistStopPoint, rectTransform.anchoredPosition.y);
                    checklistCanMove = false;
                    checklistIsMoving = false;
                    checklistReachedEnd = true;

                    checklistText.text += "\nTa-da! Checklist available";
                }
            }

            //Otherwise, if the checklist is visible, move it to the left
            else if (rectTransform.anchoredPosition.x > checklistStartPoint)
            {
                rectTransform.anchoredPosition -= new Vector2(20, 0);

                //If the checklist has reached it's starting point, stop it from moving
                if (rectTransform.anchoredPosition.x <= checklistStartPoint)
                {
                    rectTransform.anchoredPosition.Set(checklistStartPoint, rectTransform.anchoredPosition.y);
                    checklistCanMove = false;
                    checklistIsMoving = false;
                    checklistReachedEnd = false;

                    checklistText.text = "Request #1\n\r\n- Red Flower\r\n- Blue Flower\r\n- Blue Flower";
                }
            }
        }
    }

    /// <summary>
    /// When the player clicks on the checklist, move it to the side of the screen
    /// </summary>
    public void ChecklistClicked(InputAction.CallbackContext context)
    {

        if (!isActiveAndEnabled) return;

        //If the context has not been started, return (I think)
        //if (!context.canceled) return;x

        //If the checklist is not moving, continue
        if (!checklistIsMoving)
        {
            //Save the position of where the user touched the screen to the clickData var
            clickData.position = InputHelper.GetPointerPosition();
            
            //Clear the previous list of click results
            clickResults.Clear();

            //Raycast the clickData into the clickResults lisst
            graphicRaycaster.Raycast(clickData, clickResults);

            //Go through each result in clickResults
            foreach (RaycastResult result in clickResults)
            {
                //If the clicked element is the checklist collider, continue
                if (result.gameObject.name == "UICollider")
                {
                    //Set the checklistCanMove var to the opposite of what it is
                    checklistCanMove = !checklistCanMove;
                }
            }

            //Previous code from when the checklist was a game object and not yet a UI object
            //var rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
            //Debug.LogWarning(rayHit.GetType());
            //Debug.LogWarning(rayHit.collider);
            //if (!rayHit.collider) return;
        }
    }
}
