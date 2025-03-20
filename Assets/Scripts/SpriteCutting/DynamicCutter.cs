using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Generates vectors to be used to cut a sprite
/// </summary>
public class DynamicCutter : MonoBehaviour
{
    bool isMouseDown;
    Vector2 enterMousePos; // The position of the cutter when the cut begins
    Vector2 exitMousePos; // The position of the cutter when the cut finishes
    Vector2 cutLine; // The line created
    RaycastHit2D lineRaycast; // Raycast associated with the line

    public Vector2 CutLine { get => cutLine; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enterMousePos = Vector2.zero;
        exitMousePos = Vector2.zero;
        cutLine = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if(isMouseDown)
        {
            // Update cutter position
            transform.position = InputHelper.GetPointerWorldPosition();
        }
    }

    /// <summary>
    /// Called when user taps down on the screen
    /// </summary>
    /// <param name="context"></param>
    public void OnTap(InputAction.CallbackContext context)
    {
        isMouseDown = !context.canceled;
    }

    /// <summary>
    /// Updates the start of the cutting line
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollisionEnter2D(Collision2D collision)
    {
        enterMousePos = transform.position;
        Debug.Log(enterMousePos);
    }

    /// <summary>
    /// Updates the end of the cutting line, then generates a vector
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollisionExit2D(Collision2D collision)
    {
        exitMousePos = transform.position;
        cutLine = GenerateLine();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(enterMousePos, exitMousePos);
    }

    private Vector2 GenerateLine()
    {
        return exitMousePos - enterMousePos;
    }

}
