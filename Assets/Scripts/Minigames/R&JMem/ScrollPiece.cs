using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScrollPiece : MonoBehaviour
{
    public PlayerInput playerInput;

    public Draggable draggableObj;
    private bool isContaining = false;

    public GameObject puzzle;

    private Rect rect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rect = this.GetComponent<RectTransform>().rect;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (draggableObj != null && isContaining)
        {
            bool contains = rect.Contains(draggableObj.transform.localPosition);
            
            if (!contains)
            {
                isContaining = false;
                draggableObj.transform.SetParent(draggableObj.originParent.transform, true);
                ChangeAlpha(0.5f);
            }
        }
        
    }

    public void Constructor(Draggable dragObj)
    {
        if (dragObj != null)
        {
            draggableObj = dragObj;
            isContaining = true;

            this.GetComponent<Image>().sprite = dragObj.GetComponent<SpriteRenderer>().sprite;
            Color tempColor = new Color(0.3207f, 0.2708f, 0.2708f);
            this.GetComponent<Image>().color = tempColor;

            ChangeAlpha(0f);

        }
    }

    private void ChangeAlpha(float value)
    {
        Color tempColor = this.GetComponent<Image>().color;
        tempColor.a = value;
        this.GetComponent<Image>().color = tempColor;
    }


    // OnTriggerEnter2D is called when the Collider2D other enters the trigger (if the GameObject has a Collider2D component with "Is Trigger" checked)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == draggableObj.gameObject)
        {
            isContaining = true;
            draggableObj.transform.SetParent(this.transform, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == draggableObj.gameObject)
        {
            isContaining = false;
            draggableObj.transform.SetParent(draggableObj.originParent.transform, true);
        }
    }

}
