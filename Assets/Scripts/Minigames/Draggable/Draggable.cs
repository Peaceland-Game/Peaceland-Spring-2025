using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Component for draggable objects. Requires a Drag Manager.

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Draggable : MonoBehaviour
{
    /// <summary>
    /// Targets for the object to snap to
    /// </summary>
    [SerializeField]
    private List<Transform> dragTargets;

    /// <summary>
    /// Limit the object to stay in this area
    /// </summary>
    [SerializeField]
    private BoxCollider2D dragLimit;

    /// <summary>
    /// How close to get to a drag target before snapping to it=
    /// </summary>
    [SerializeField]
    private float dragDistanceThreshold = 2.0f;

    /// <summary>
    /// When the object is released on a drag target, this signal is invoked with the transform of the target.
    /// </summary>
    [SerializeField]
    UnityEvent<Transform> draggedOnTargetEvent;

    /// <summary>
    /// Can we drag this object?
    /// </summary>
    private bool draggable = true;

    /// <summary>
    /// Are we dragging this object?
    /// </summary>
    private bool dragging = false;

    /// <summary>
    /// Offset from mouse/touch when dragging
    /// </summary>
    private Vector3 offset;

    /// <summary>
    /// What target we're snapping to
    /// </summary>
    private int snapIndex = -1;

    /// <summary>
    /// Tepresents the flower type enum value from order object
    /// </summary>
    FlowerType typeofFlower;
    
    /// <summary>
    /// Collison for object
    /// </summary>
    private Collider2D bounds;

    /// <summary>
    /// RigidBody2D reference
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// get starting position for draggable
    /// </summary>
    private Vector3 startPos;

    void Start()
    {   
        bounds = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    // Called when object is instantiated
    public void Constructor(GameObject[] _dragTargets, FlowerType _typeOfFlower)
    {
        foreach (GameObject dragTarget in _dragTargets)
        {
            dragTargets.Add(dragTarget.transform);
        }
        typeofFlower = _typeOfFlower;

        // Set the sprite
        GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetFlowerSprite(_typeOfFlower);
    }

    public bool CanDrag(Vector3 touch_wp) {
        return draggable && bounds.OverlapPoint(touch_wp);
    }

    /// <summary>
    /// Is this draggable object draggable?
    /// </summary>
    /// <returns>If the current draggable is indeed draggable or not</returns>
    public bool IsDraggable()
    {
        return draggable;
    }

    /// <summary>
    /// We start dragging this object
    /// </summary>
    /// <param name="touch_wp">Touch world position</param>
    public void StartDrag(Vector3 touch_wp) {
        dragging = true;
        offset = transform.position - touch_wp;
    }

    /// <summary>
    /// Drag disabled
    /// </summary>
    public void DisableDrag() {
        draggable = false;
    }

    /// <summary>
    /// Drag enabled
    /// </summary>
    public void EnableDrag() {
        draggable = true;
    }

    /// <summary>
    /// Player stopped dragging this object
    /// </summary>
    public void EndDrag() {
        // End drag
        dragging = false;
        BoundsCheck();

        if (snapIndex != -1) {
            DisableDrag();
            dragTargets[snapIndex].GetComponent<DragTarget>().isSnapped = true;
            draggedOnTargetEvent.Invoke(dragTargets[snapIndex]);
        }
    } 

    public void BoundsCheck()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.transform.position.z);
        Vector3 screenHeight = new Vector3(Screen.width / 2, Screen.height, Camera.main.transform.position.z);
        Vector3 screenWidth = new Vector3(Screen.width, Screen.height / 2, Camera.main.transform.position.z);
               
        Vector3 goScreen = Camera.main.WorldToScreenPoint(transform.position);

        float distX = Vector3.Distance(new Vector3(Screen.width / 2, 0f, 0f), new Vector3(goScreen.x, 0f, 0f));
        float distY = Vector3.Distance(new Vector3(0f, Screen.height, 0f), new Vector3(0f, goScreen.y, 0f));

        if (distX > Screen.width / 2 || distY > Screen.height / 2)
        {
            transform.position = startPos;
        }
        
    }

    void Update()
    {
        if (!draggable || !dragging) return;

        Vector3 touch_wp = InputHelper.GetPointerWorldPosition();

        if (dragging)
        {
            Vector3 newPos = new(touch_wp.x + offset.x, touch_wp.y + offset.y, transform.position.z);
            Vector3 newRot = new(0, 0, 0);

            // Snap to closts drag target (if one exists)
            if (dragTargets.Count > 0) {
                float lowest_dist = float.MaxValue;
                int i = 0;
                snapIndex = -1;
                foreach (Transform target in dragTargets) {
                    if (!target.GetComponent<DragTarget>().isSnapped)
                    {
                        float dist = (newPos - target.position).magnitude;
                        // Snap position and rotation if close enough AND if their flower types are the same
                        if (dist < dragDistanceThreshold && dist < lowest_dist && target.gameObject.GetComponent<DragTarget>().TypeOfFlower == typeofFlower)
                        {
                            newPos = target.position;
                            newRot = target.eulerAngles;
                            snapIndex = i;
                        }
                    }
                    i++;
                }
            }

            // Clamp position to drag limit
            if (!dragLimit || dragLimit.OverlapPoint(newPos)) {
                transform.position = newPos;
                transform.eulerAngles = newRot;
            }

        }

    }
}
