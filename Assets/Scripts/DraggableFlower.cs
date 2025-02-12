using System;
using UnityEngine;

public class DraggableFlower : MonoBehaviour
{
    [SerializeField]
    private Transform dragTarget;

    private bool dragging = false;
    private Vector3 offset;
    
    private BoxCollider2D boxCollider2D;

    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    void Update()
    {

        if (Input.touchCount != 1)
        {
            dragging = false;
            return;
        }

        Touch touch = Input.GetTouch(0);

        Vector3 touch_wp = Camera.main.ScreenToWorldPoint(touch.position);

        if (touch.phase == TouchPhase.Began)
        {
            if (GetComponent<Collider2D>().OverlapPoint(touch_wp))
            {
                dragging = true;
                offset = transform.position - touch_wp;
            }
        }
        else if (dragging && touch.phase == TouchPhase.Moved)
        {
            transform.position = new Vector3(touch_wp.x + offset.x, touch_wp.y + offset.y, transform.position.z);

            float dist = (transform.position - dragTarget.position).magnitude;
            if (dist < 2.0) {
                transform.position = dragTarget.position;
            }

        }
        else if (dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
        {
            dragging = false;
        }
    }
}
