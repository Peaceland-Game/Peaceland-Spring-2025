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
    Vector2 line; // The line created
    RaycastHit2D lineRaycast; // Raycast associated with the line

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        enterMousePos = transform.position;
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        exitMousePos = transform.position;
        line = GenerateLine();
    }

    public void OnDrawGizmos()
    {
        // TO-DO: Draw the generated line
    }

    private Vector2 GenerateLine()
    {
        return exitMousePos - enterMousePos;
    }

}
