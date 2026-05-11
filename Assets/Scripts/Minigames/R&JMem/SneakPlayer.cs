using UnityEngine;

public class SneakPlayer : MonoBehaviour
{
    // Fields
    [SerializeField]
    private float speed;                        // The player's base speed
    [SerializeField]
    private float speedScalar = 2.0f;           // The speed boost gained by sprinting
    private bool isHiding = false;              // Checks if the player is currently pressing the HIDE button
    private bool isWithinHidingPlace = false;   // Checks if the player is in range of a hiding spot
    private bool isSprinting = false;           // Checks if the player is currently sprinting
    private bool isCaught = false;              // Checks if the player has been caught

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
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    /// <summary>
    /// Logic when the player enters the collider of another game object
    /// </summary>
    /// <param name="collision">The object the player collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When the player enters a hiding place, swich bool to true
        if (collision.CompareTag("HidingPlace"))
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
        if (collision.CompareTag("HidingPlace"))
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
                gameObject.transform.Translate(speed * speedScalar, 0, 0);
            }
            else
            {
                gameObject.transform.Translate(speed, 0, 0);
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
}
