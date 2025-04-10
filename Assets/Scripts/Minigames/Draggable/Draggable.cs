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
    [SerializeField]
    private List<Transform> dragTargets; // Targets for the object to snap to

    [SerializeField]
    private BoxCollider2D dragLimit; // Limit the object to stay in this area

    [SerializeField]
    private float dragDistanceThreshold = 2.0f; // How close to get to a drag target before snapping to it 

    [SerializeField]
    UnityEvent<Transform> draggedOnTargetEvent; // When the object is released on a drag target, this signal is invoked with the transform of the target.

    private bool draggable = true;

    private bool dragging = false;
    private Vector3 offset;
    private int snapIndex = -1;

    /// <summary>
    /// represents the flower type enum value from order object
    /// </summary>
    FlowerType typeofFlower;
    
    private Collider2D bounds;
    void Start()
    {   
        bounds = GetComponent<Collider2D>();
    }

    //called when object is instantiated
    public void Constructor(GameObject[] _dragTargets, FlowerType _typeOfFlower)
    {
        foreach (GameObject dragTarget in _dragTargets)
        {
            dragTargets.Add(dragTarget.transform);
        }
        typeofFlower = _typeOfFlower;

        //set the sprite
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

    public void StartDrag(Vector3 touch_wp) {
        dragging = true;
        offset = transform.position - touch_wp;
    }

    public void DisableDrag() {
        draggable = false;
    }

    public void EnableDrag() {
        draggable = true;
    }

    public void EndDrag() {
        // End drag
        dragging = false;

        if (snapIndex != -1) {
            DisableDrag();
            dragTargets[snapIndex].GetComponent<DragTarget>().isSnapped = true;
            draggedOnTargetEvent.Invoke(dragTargets[snapIndex]);
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
                        //snap position and rotation if close enough AND if their flower types are the same
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
