using UnityEngine;

public class SneakSentry : MonoBehaviour
{
    [SerializeField]
    private float speed;    // The speed of the sentry
    private SpriteRenderer spriteRenderer;  //The sentry's spriteRenderer

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 5;
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
        gameObject.transform.Translate(-speed * Time.deltaTime, speed * Time.deltaTime, 0);
    }
}
