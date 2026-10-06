using UnityEngine;
using UnityEngine.InputSystem;

public class SneakPlayer : MonoBehaviour
{
    // Fields
    [SerializeField]
    private float speed;                        // The player's base speed
    [SerializeField]
    private float speedScalar = 2.0f;           // The speed boost gained by sprinting
    [SerializeField]
    private SpriteRenderer spriteRenderer;      // The SpriterRenderer for the player
    private bool isHiding = false;              // Checks if the player is currently pressing the HIDE button
    private bool isWithinHidingPlace = false;   // Checks if the player is in range of a hiding spot
    private bool isSprinting = false;           // Checks if the player is currently sprinting
    private bool isCaught = false;              // Checks if the player has been caught
    private CircleCollider2D fullBodyCollider;  // Collider for the player sprite, for detecting if they're in an enemy vision cone
    private CircleCollider2D innerBodyCollider; // Collider for the middle of the player sprite, for detecting if they're sufficiently within hiding spots

    private Camera cam;         // Reference to the camera
    [SerializeField]
    private float camOffset;    // Distance to offset player from Camera

    // Get/Set Properties
    /// <summary>
    /// Returns whether or not the player has been caught
    /// </summary>
    public bool IsCaught { get { return isCaught; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find camera
        cam = FindFirstObjectByType<Camera>();

        // Set player starting postion to camera location minus offset
        gameObject.transform.position = new Vector3(cam.transform.position.x - camOffset, 0);
        // Reset all values
        isWithinHidingPlace = false;
        isHiding = false;
        isSprinting = false;
        isCaught = false;

        //find colliders
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();

        //as long as there actually is 2 or more, assign them to full body and innerbody
        if (colliders.Length >= 2)
        {
            //Unity gets them from top to bottom in the inspector, in this case that means the full body one is first and the inner one is second.
            fullBodyCollider = colliders[0];
            innerBodyCollider = colliders[1];
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Check that a keyboard is connected as a precaution
        if (Keyboard.current == null) return;

        //assuming a keyboard is connected, check if shift or control was pressed
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            //if shift was pressed, start sprinting, and stop hiding to prevent issues where you're sprinting and hiding at the same time
            isSprinting = true;
            isHiding = false;

        }
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            //if ctrl was pressed, vice versa, but also change the sprite layer to show the character is in shadows when appropriate
            isSprinting = false;
            isHiding = true;
          
        }

        //then check if either key was released this frame
        if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
        {
            //if shift was released, stop sprinting
            isSprinting = false;

        }
        if (Keyboard.current.leftCtrlKey.wasReleasedThisFrame)
        {
            //if ctrl was released, stop hiding and return sprite layer to normal
            isHiding = false;
           
        }

        //check to see if the sprite needs darkened or not
        Darken();
    }

    /// <summary>
    /// Logic when the player enters the collider of another game object
    /// </summary>
    /// <param name="collision">The object the player collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When the player enters a hiding place, swich bool to true
        if (innerBodyCollider.IsTouching(collision) && collision.CompareTag("HidingPlace"))
        {
            isWithinHidingPlace = true;
            Debug.Log("Player is in a hiding place.");
        }
    }

    /// <summary>
    /// Logic when the player exits the collider of another game object
    /// </summary>
    /// <param name="collision">The object the player collided with</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // When the player leaves a hiding place, swich bool to false
        if (!innerBodyCollider.IsTouching(collision) && collision.CompareTag("HidingPlace"))
        {
            isWithinHidingPlace = false;
            Debug.Log("Player has left a hiding place.");
        }
    }

    /// <summary>
    /// Handles logic when the player stays inside a collider
    /// </summary>
    /// <param name="collision">The collider of the objects the player is colliding with.</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        // When the player is in a sight beam
        if (collision.gameObject.GetComponent<SneakSentry>() != null)
        {
            if (isWithinHidingPlace && isHiding)
            {
                Debug.Log("Player hid from Sentry");
            }
            else
            {
                Debug.Log("Player caught by Sentry");
                isCaught = true;
            }
        }
    }

    /// <summary>
    /// If the player is not hiding, move the sprite with the camera
    /// </summary>
    public void MovePlayer()
    {
        if (!isHiding)
        {
            // Move the player forward
            if (isSprinting)
            {
                gameObject.transform.Translate(speed * speedScalar * Time.deltaTime, 0, 0);
            }
            else
            {
                gameObject.transform.Translate(speed * Time.deltaTime, 0, 0);
            }

        }
    }

    /// <summary>
    /// Sets the bool that determines if the player is hiding.
    /// </summary>
    /// <param name="_isHiding"> Boolean for if the player is hiding</param>
    public void SetHide(bool _isHiding)
    {
        isHiding = _isHiding;
    }

    /// <summary>
    /// Sets the bool that determines if the player is sprinting.
    /// </summary>
    /// <param name="_isSprinting">Boolean for if the player is springing</param>
    public void SetSprinting(bool _isSprinting)
    {
        isSprinting = _isSprinting;
    }

    /// <summary>
    /// Resets all player values when restarting minigame
    /// </summary>
    public void ResetPlayer()
    {
        gameObject.transform.position = new Vector3(cam.transform.position.x - camOffset, 0);
        isHiding = false;
        isWithinHidingPlace = false;
        isCaught = false;
    }

    /// <summary>
    /// Darkens the character sprite when hiding and lightens it back to normal at all other times
    /// </summary>
    public void Darken()
    {
        //if succesfully hidden, darken the sprite to show as such, if just hitting the hide button but not in a hiding spot (or vice versa), don't darken.
        if (isWithinHidingPlace && isHiding)
        {
            spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        }
        else
        {
            //return the sprite's brightness to normal. This won't make any visible change if they were already normal 
            //brightness, but guarantees that they return to full brightness whenever they arent hiding regardless of location
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }
}
