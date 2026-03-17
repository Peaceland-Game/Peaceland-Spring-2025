using UnityEngine;

public class SneakSentry : MonoBehaviour
{
    [SerializeField]
    private float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move()
    {
        gameObject.transform.Translate(-speed, 0, 0);
    }
}
