using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabAndSwipe : MonoBehaviour
{
    /// <summary>
    /// the speed the shears need to be moving (units/sec) in order to slice
    /// </summary>
    [SerializeField] float sliceSpeed;

    bool isMouseDown;
    bool isSlicing;
    Vector2 previousMousePos;
    SpriteRenderer spriteRenderer;

    public bool IsSlicing { get { return isSlicing; } }

    private void Start()
    {
        isSlicing = false;
        previousMousePos = Vector2.zero;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(isMouseDown)
        {
            // Snap to mouse position
            transform.position = InputHelper.GetPointerWorldPosition();

            // Determine if the blade is slicing
            isSlicing = Vector2.Distance(previousMousePos, transform.position) >= sliceSpeed * Time.deltaTime;

            // Update prev mouse pos
            previousMousePos = transform.position;
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
}
