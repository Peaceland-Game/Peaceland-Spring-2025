using UnityEngine;

public class SneakPlayer : MonoBehaviour
{
    // Fields
    [SerializeField]
    private float speed;
    private float speedScalar = 1.0f;    // Allows the player to catch up to the player.
    private bool isHiding = false;

    private Camera cam;
    [SerializeField]
    private float camOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find camera
        cam = FindFirstObjectByType<Camera>();

        // Set player starting postion to camera location minus offset
        gameObject.transform.position = new Vector3(cam.transform.position.x - camOffset, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    /// <summary>
    /// If the player is not hiding, move the sprite with the camera
    /// </summary>
    public void MovePlayer()
    {
        if (!isHiding)
        {
            // If the player is lagging behind the camera, scale the speed
            if (gameObject.transform.position.x < cam.transform.position.x - camOffset)
            {
                speedScalar = CalculateSpeedScalar();
                Debug.Log("Speed Scalar: " + speedScalar);
            }
            else
            {
                speedScalar = 1.0f;
            }

            // Move the player forward
            gameObject.transform.position = new Vector3(gameObject.transform.position.x + speed * speedScalar,
                   gameObject.transform.position.y);
        }
    }

    /// <summary>
    /// Increase the player's speed if they are further from the camera
    /// </summary>
    /// <returns>A float to scale the player's speed by when they are far from the camera.</returns>
    private float CalculateSpeedScalar()
    {
        return 1 + (cam.transform.position.x - gameObject.transform.position.x) / 2.0f;
        // Note: perhaps this should grant a flat speed boost instead of a scalar
    }

    /// <summary>
    /// Sets the bool that determines if the player is hiding.
    /// </summary>
    /// <param name="_isHiding"> Boolean for if the player is hiding</param>
    public void SetHide(bool _isHiding)
    {
        isHiding = _isHiding;
    }


    public void ResetPlayer()
    {
        gameObject.transform.position = new Vector3(cam.transform.position.x - camOffset, 0);
        isHiding = false;
        speedScalar = 1.0f;
    }
}
