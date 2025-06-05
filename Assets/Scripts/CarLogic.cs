using UnityEngine;

public class CarLogic : MonoBehaviour
{
    [SerializeField]
    private float startingX; // X pos car should start at

    [SerializeField]
    private float endingX; // X pos car should end at

    [SerializeField]
    private float minY; // Lowest point car can show

    [SerializeField]
    private float maxY; // Highest point car can show

    [SerializeField]
    private float speed; // How fast car goes

    [SerializeField]
    private float minWaitTime; // Minimum time between cars appearing

    [SerializeField]
    private float maxWaitTime; // Maximum time between cars appearing

    private float timestamp; // Used to track how long to wait

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setWait();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        pos.x += speed * Time.deltaTime; //Moves X along by speed
        if (pos.x > endingX)
        { // If posisition is past ending
            if (Time.time > timestamp)
            { // Teleport to start if ready
                this.GetComponent<SpriteRenderer>().enabled = true;
                pos.x = startingX;
                pos.y = Random.Range(minY, maxY); // Random Y for extra variance
                setWait();
            }
            else
            { //Otherwise hide
                this.GetComponent<SpriteRenderer>().enabled = false;
            }  
        }
        transform.position = pos;
    }

    /// <summary>
    /// Randomly generates how long it will take for the car to appear again
    /// </summary>
    void setWait()
    {
        timestamp = Time.time + Random.Range(minWaitTime, maxWaitTime);
    }
}
