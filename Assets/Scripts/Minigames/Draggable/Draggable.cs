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
    
    private Collider2D bounds;
    void Start()
    {   
        bounds = GetComponent<Collider2D>();
    }

    //called when object is instantiated
    public void Constructor(GameObject[] _dragTargets)
    {
        foreach (GameObject dragTarget in _dragTargets)
        {
            dragTargets.Add(dragTarget.transform);
        }
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

            // Snap to closts drag target (if one exists)
            if (dragTargets.Count > 0) {
                float lowest_dist = float.MaxValue;
                int i = 0;
                snapIndex = -1;
                foreach (Transform target in dragTargets) {
                    float dist = (newPos - target.position).magnitude;
                    if (dist < dragDistanceThreshold && dist < lowest_dist) {
                        newPos = target.position;
                        snapIndex = i;
                    }
                    i++;
                }
            }

            // Clamp position to drag limit
            if (!dragLimit || dragLimit.OverlapPoint(new Vector2(newPos.x, newPos.y))) {
                transform.position = newPos;
            }

        }
    }
}
