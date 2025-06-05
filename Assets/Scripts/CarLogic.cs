using UnityEngine;

public class CarLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    void Start()
    {
        setWait();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        pos.x += speed * Time.deltaTime;
        if (pos.x > endingX)
        {
            if (Time.time > timestamp)
            {
                this.GetComponent<SpriteRenderer>().enabled = true;
                pos.x = startingX;
                pos.y = Random.Range(minY, maxY);
                setWait();
            }
            else
            {
                this.GetComponent<SpriteRenderer>().enabled = false;
            }  
        }
        transform.position = pos;
    }

    void setWait()
    {
        timestamp = Time.time + Random.Range(minWaitTime, maxWaitTime);
    }
}
