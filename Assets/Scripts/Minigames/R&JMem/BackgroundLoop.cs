using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    //variables
    private Transform cameraTransform;
    private float spriteWidth;
    private float buffer;
    private Vector3 startPos;
    [SerializeField]
    private float speed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get the camera
        cameraTransform = Camera.main.transform;

        //get the width of the sprite automatically (Also accounts for scaling)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteWidth = spriteRenderer.bounds.size.x;
        }
        startPos = transform.position;

        //set the buffer
        buffer = spriteWidth / 20;

    }

    // Update is called once per frame
    void Update()
    {
        //when the camera passes the center of the sprite + the sprites width
        if (cameraTransform.position.x - transform.position.x >= spriteWidth )
        {
            //move the sprite two widths ahead, a.k.a ahead of the camera to keep the loop going
            transform.position = new Vector3(transform.position.x + (spriteWidth * 2) - buffer, transform.position.y, transform.position.z);
        }
        Move();
    }

    /// <summary>
    /// Returns the background to its starting position to reset the scene
    /// </summary>
    public void ResetBackground()
    {
        transform.position = startPos;
    }

    /// <summary>
    /// Moves the item in question to the left according to its speed for parallax purposes, default speed to 0 if you want it moving in sync with the camera.
    /// (Speed divided by 200 to make it so the numbers in inspector are reasonable instead of all being 0.0X)
    /// </summary>
    public void Move()
    {
        transform.position = new Vector3(transform.position.x - (speed/200), transform.position.y, transform.position.z);
    }
}
