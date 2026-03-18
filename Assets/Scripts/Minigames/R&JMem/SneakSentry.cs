using UnityEngine;

public class SneakSentry : MonoBehaviour
{
    [SerializeField]
    private float speed;    // The speed of the sentry

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Moves the sentry based on its speed
    /// </summary>
    public void Move()
    {
        // Speed is negative because sentries move right to left
        gameObject.transform.Translate(-speed, 0, 0);
    }
}
