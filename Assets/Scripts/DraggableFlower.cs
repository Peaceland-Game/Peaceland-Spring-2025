using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DraggableFlower : MonoBehaviour
{
    [SerializeField]
    private List<Transform> dragTargets; // Targets for the object to snap to

    [SerializeField]
    private BoxCollider2D dragLimit; // Limit the object to stay in this area

    [SerializeField]
    private float dragDistanceThreshold = 2.0f; // How close to get to a drag target before snapping to it 

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

        TouchControl touch = Touchscreen.current.primaryTouch;

        Vector3 touch_wp = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());

        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            if (GetComponent<Collider2D>().OverlapPoint(touch_wp))
            {
                dragging = true;
                offset = transform.position - touch_wp;
            }
        }
        else if (dragging && touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector3 newPos = new Vector3(touch_wp.x + offset.x, touch_wp.y + offset.y, transform.position.z);

            // Snap to closts drag target (if one exists)
            if (dragTargets.Count > 0) {
                float lowest_dist = float.MaxValue;
                foreach (Transform target in dragTargets) {
                    float dist = (newPos - target.position).magnitude;
                    if (dist < dragDistanceThreshold && dist < lowest_dist) {
                        newPos = target.position;
                    }
                }
            }

            // Clamp position to drag limit
            if (!dragLimit || dragLimit.OverlapPoint(new Vector2(newPos.x, newPos.y))) {
                transform.position = newPos;
            }

        }
        else if (dragging && (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled))
        {
            dragging = false;
        }
    }
}
