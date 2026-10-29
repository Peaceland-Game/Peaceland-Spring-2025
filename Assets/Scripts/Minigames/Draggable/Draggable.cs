using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Transactions;

using UnityEngine.UI;

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
    private float dragDistanceThreshold = 3.5f;

    /// <summary>
    /// When the object is released on a drag target, this signal is invoked with the transform of the target.
    /// </summary>
    [SerializeField]
    UnityEvent<Transform> draggedOnTargetEvent;

    [SerializeField]
    private bool allowRemovalFromTarget = true;

    ///[SerializeField] 
    private int boundsOffset = 0;

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
    /// Stores the type of the current draggable, used for checking if it can snap to a target
    /// </summary>
    private object dragType;
    
    /// <summary>
    /// Collison for object
    /// </summary>
    private Collider2D bounds;

    private int difficulty;
    private float shakeTimer;
    private float xShakeOffset;
    private float yShakeOffset;

    /// <summary>
    /// RigidBody2D reference
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// get starting position for draggable
    /// </summary>
    private Vector3 startPos;

    /// <summary>
    /// used to check screen bounds for draggable objects in BoundsCheck()
    /// </summary>
    private Renderer renderer;
    private Camera camera;

    /// <summary>
    /// drag manager ref
    /// </summary>
    public DragManager dm;

    public bool IsDragging { get; }
    public DragManager originParent;


    void Start()
    {   
        dm = FindFirstObjectByType<DragManager>();

        bounds = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        renderer = GetComponent<Renderer>();
        camera = FindFirstObjectByType<Camera>();
        
    }

    // Called when object is instantiated
    public void Constructor<T>(GameObject[] _dragTargets, T _dataType, Sprite sprite = null)
    {
        if (dragTargets == null) dragTargets = new List<Transform>();
        dragTargets.Clear();
        foreach (GameObject dragTarget in _dragTargets)
        {
            if (dragTarget != null) dragTargets.Add(dragTarget.transform);
        }
        dragType = _dataType;
        startPos = transform.position;

        // Set the sprite
        
        if (sprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

        //fixed image box collider not matching the piece size by setting the size of the box collider to match the sprite size
        SpriteRenderer sr = this.GetComponent<SpriteRenderer>();
        BoxCollider2D bc = this.GetComponent<BoxCollider2D>();
        if (sr != null && bc != null)
        {
            Debug.Log("box2d size: " + bc.size);
            Debug.Log(sr.sprite.name + " sprite size: " + sr.sprite.bounds.size);
            bc.size = new Vector2(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
            //bc.offset = sr.sprite.bounds.center;
        }

    }

    public bool CanDrag(Vector3 touch_wp) {
        if (bounds == null) bounds = GetComponent<Collider2D>();
        bool canInteract = draggable || (allowRemovalFromTarget && snapIndex != -1);
        return canInteract && bounds != null && bounds.OverlapPoint(touch_wp);
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
    public void StartDrag(Vector3 touch_wp, int currentDifficulty)
    {
        dragging = true;
        offset = transform.position - touch_wp;
        difficulty = currentDifficulty;

        if (snapIndex != -1 && allowRemovalFromTarget)
        {
            if (snapIndex < dragTargets.Count && dragTargets[snapIndex] != null)
            {
                var dt = dragTargets[snapIndex].GetComponent<DragTarget>();
                if (dt != null) dt.UnsnapPiece(this);
            }
            snapIndex = -1;
            if (dm != null && dm.completedDragCount > 0)
            {
                dm.completedDragCount -= 1;
            }
            EnableDrag();
        }

        GameObject scrollView = GameObject.FindGameObjectWithTag("ScrollView");
        if (scrollView != null)
        {
            scrollView.GetComponent<ScrollRect>().enabled = false;
        }

        GameObject gameObj = this.gameObject;
        if (gameObj != null && gameObj.transform.parent.CompareTag("ScrollContent"))
        {
            gameObj.transform.SetParent(GameObject.FindGameObjectWithTag("Minigame").transform, true);
        }
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

    public void ReturnToStartPos()
    {
        if (snapIndex != -1 && snapIndex < dragTargets.Count && dragTargets[snapIndex] != null)
        {
            var dt = dragTargets[snapIndex].GetComponent<DragTarget>();
            if (dt != null && dt.currentSnappedPiece == this)
            {
                dt.UnsnapPiece(this);
            }
        }
        snapIndex = -1;
        dragging = false;
        EnableDrag();
        transform.position = startPos;
    }

    /// <summary>
    /// Player stopped dragging this object
    /// </summary>
    public void EndDrag() {
        // End drag
        dragging = false;

        GameObject scrollView = GameObject.FindGameObjectWithTag("ScrollView");
        if (scrollView!=null)
        {
            scrollView.GetComponent<ScrollRect>().enabled = true;
        }

        if (snapIndex == -1 && dragTargets != null && dragTargets.Count > 0)
        {
            float lowestDist = float.MaxValue;
            int foundIndex = -1;
            for (int i = 0; i < dragTargets.Count; i++)
            {
                Transform target = dragTargets[i];
                if (target != null && target.GetComponent<DragTarget>().CanSnap(dragType))
                {
                    float dist = (transform.position - target.position).magnitude;
                    if (dist < dragDistanceThreshold && dist < lowestDist)
                    {
                        lowestDist = dist;
                        foundIndex = i;
                    }
                }
            }
            if (foundIndex != -1)
            {
                snapIndex = foundIndex;
                transform.position = dragTargets[snapIndex].position;
                transform.eulerAngles = dragTargets[snapIndex].eulerAngles;
                var dt = dragTargets[snapIndex].GetComponent<DragTarget>();
                bool wasAlreadySnapped = (dt != null && dt.isSnapped);
                if (dt != null)
                {
                    dt.SnapPiece(this);
                }
                if (dm != null && !wasAlreadySnapped)
                {
                    dm.completedDragCount += 1;
                    dm.CompletionCheck();
                }
            }
        }

        if (snapIndex != -1) {
            if (!allowRemovalFromTarget) DisableDrag();
            draggedOnTargetEvent.Invoke(dragTargets[snapIndex]);
        }
        else
        {
            // Dropped outside any valid target: return smoothly back to original position in the word bank!
            transform.position = startPos;
        }

        
    }

    public void setBoundsOffset(int offset)
    {
        this.boundsOffset = offset;
    }
    /// <summary>
    /// check if object is off screen, return to starting position if it is
    /// </summary>
    public void BoundsCheck()
    {
        Vector3 screenpos = camera.WorldToScreenPoint(transform.position);
        bool onScreen = screenpos.x > 0f && screenpos.x < (Screen.width -boundsOffset) && screenpos.y > 0f && screenpos.y < Screen.height;

        if (onScreen && renderer.isVisible)
        {
            return;
        }
        else
        {
            transform.position = startPos;
        }

    }

    //public void BoundsCheck()
    //{
    //    Vector3 screenpos = camera.WorldToScreenPoint(transform.position);
    //    bool onScreen = screenpos.x > 0f && screenpos.x < Screen.width && screenpos.y > 0f && screenpos.y < Screen.height;

    //    if (onScreen && renderer.isVisible)
    //    {
    //        return;
    //    }
    //    else
    //    {
    //        transform.position = startPos;
    //    }

    //}

    void Update()
    {
        if (!draggable || !dragging) return;
        
        Vector3 touch_wp = InputHelper.GetPointerWorldPosition();

        if (dragging)
        {
            //Shakes the cursor if the difficulty is above 0
            if (difficulty > 0)
            {
                shakeTimer += Time.deltaTime;
                if (shakeTimer > 0.2)
                {
                    xShakeOffset = UnityEngine.Random.Range(-0.4f, 0.4f);
                    yShakeOffset = UnityEngine.Random.Range(-0.4f, 0.4f);
                    shakeTimer = 0;
                }
            }

            //Slowly resets the shake in between calls
            xShakeOffset *= 0.99f;
            yShakeOffset *= 0.99f;

            Vector3 newPos = new(touch_wp.x + offset.x + xShakeOffset, touch_wp.y + offset.y + yShakeOffset, transform.position.z);
            Vector3 newRot = new(0, 0, 0);
           
            // Clamp position to drag limit
            if (!dragLimit || dragLimit.OverlapPoint(newPos)) {
                transform.position = newPos;
                transform.eulerAngles = newRot;
            }

        }
    }
}
