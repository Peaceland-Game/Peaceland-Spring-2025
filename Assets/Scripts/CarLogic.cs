using UnityEngine;

public class CarLogic : MonoBehaviour
{
    [SerializeField]
    private float leftX; // X pos car should start at

    [SerializeField]
    private float rightX; // X pos car should end at

    [SerializeField]
    private float minY; // Lowest point car can show

    [SerializeField]
    private float maxY; // Highest point car can show

    [SerializeField]
    private float minSpeed; // How fast car goes at slowest

    [SerializeField]
    private float maxSpeed; // How fast car goes at fastest

    [SerializeField]
    private float minWaitTime; // Minimum time between cars appearing

    [SerializeField]
    private float maxWaitTime; // Maximum time between cars appearing

    [SerializeField]
    private Sprite[] carSprites; //All car sprites that can be used

    private float speed;
    private float timestamp; // Used to track how long to wait
    private float midY; // Average Y value to track what side of road
    private Vector3 scale; // What the scale is when going right
    private Vector3 flipScale; // What the scale is when going left
    private bool right; // If the car is going right
    private bool done; // If the car has passed its target

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        done = false;
        midY = (maxY + minY) / 2f;
        scale = transform.localScale;
        flipScale = scale;
        flipScale.x *= -1;
        transform.position=prepCar();
    }

    // Update is called once per frame
    void Update()
    {
        done = false;
        Vector3 pos = transform.position;
        if (right)
        {
            pos.x += speed * Time.deltaTime; //Moves X along by speed
            if (pos.x > rightX)
            { // If posisition is past ending
                done = true;
            }
        }
        else
        {
            pos.x += speed * -1 * Time.deltaTime; //Moves X along by speed
            if(pos.x < leftX)
            {
                done = true;
            }
        }
        if (done)
        {
            if (Time.time > timestamp)
            { // Teleport to start if ready
                this.GetComponent<SpriteRenderer>().enabled = true;
                pos = prepCar();
            }
            else
            { //Otherwise hide
                this.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        transform.position = pos;
    }

    /// <summary>
    /// Randomly generates certain aspects of the car to have variance
    /// </summary>
    private Vector3 prepCar()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        float yPos = Random.Range(minY, maxY);
        float xPos;
        if (yPos < midY)
        {
            right = true;
            xPos = leftX;
            transform.localScale = scale;
        }
        else
        {
            right = false;
            xPos = rightX;
            transform.localScale = flipScale;
        }
        timestamp = Time.time + Random.Range(minWaitTime, maxWaitTime);
        transform.GetComponent<SpriteRenderer>().sprite = carSprites[Random.Range(0, carSprites.Length)];
        return new Vector3(xPos,yPos,0);
    }
}
