using UnityEngine;

public class SneakPlayer : MonoBehaviour
{
    // Fields
    [SerializeField]
    private float speed;
    private float speedScalar = 1;    // Allows the player to catch up to the player.

    private Camera cam;
    [SerializeField]
    private float camOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find camera
        cam = FindFirstObjectByType<Camera>();

        // TODO: set player starting postion to camera location minus offset
    }

    // Update is called once per frame
    void FixedUpdate()
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

    /// <summary>
    /// Increase the player's speed if they are further from the camera
    /// </summary>
    /// <returns>A float to scale the player's speed by when they are far from the camera.</returns>
    private float CalculateSpeedScalar()
    {
        return 1 + (cam.transform.position.x - gameObject.transform.position.x);
        // return 2.0f;
    }
}
