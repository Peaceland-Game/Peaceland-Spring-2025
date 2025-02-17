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
            transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);

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

    private void OnMouseDrag()
    {
        transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);
        if (Vector2.Distance(previousMousePos, transform.position) / Time.deltaTime >= sliceSpeed)
        {
            isSlicing = true;
            spriteRenderer.color = Color.red;
        }
        else
        {
            isSlicing = false;
            spriteRenderer.color = Color.white;
        }
        previousMousePos = transform.position;
    }

}
