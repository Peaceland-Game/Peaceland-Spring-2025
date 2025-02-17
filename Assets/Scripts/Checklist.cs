using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Checklist : MonoBehaviour
{
    /// <summary>
    /// The x-coordinate where the checklist should start
    /// </summary>
    private int checklistStartPoint;

    /// <summary>
    /// The x-coordinate for where the checklist should stop moving
    /// </summary>
    private int checklistStopPoint = -300;

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

    private PointerEventData clickData;
    private List<RaycastResult> clickResults;

    private GraphicRaycaster graphicRaycaster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        checklistStartPoint = (int)rectTransform.anchoredPosition.x;

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
                rectTransform.anchoredPosition += new Vector2(10, 0);

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
                rectTransform.anchoredPosition -= new Vector2(10, 0);

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
        if (!context.started) return;

        if (!checklistIsMoving)
        {
            clickData.position = Mouse.current.position.ReadValue();
            if (clickData.position == new Vector2(0, 0))
            {
                clickData.position = Touchscreen.current.position.ReadValue();
            }
            Debug.LogWarning(clickData.position);
            clickResults.Clear();

            graphicRaycaster.Raycast(clickData, clickResults);

            foreach (RaycastResult result in clickResults)
            {
                if (result.gameObject.name == "UICollider")
                {
                    checklistCanMove = !checklistCanMove;
                }
            }

            //var rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
            //Debug.LogWarning(rayHit.GetType());
            //Debug.LogWarning(rayHit.collider);
            //if (!rayHit.collider) return;
        }
    }
}
