using UnityEngine;

public class FlowerLogic : MonoBehaviour
{
    [SerializeField] GrabAndSwipe shears;
    [SerializeField] float gravScale;
    [SerializeField] float popForce;
    [SerializeField] GameObject centerOfGravity;

    private Rigidbody2D rb;
    private Rigidbody2D centerRB;

    private bool cut;

    private void Start()
    {
        cut = false;
        rb = GetComponent<Rigidbody2D>();
        centerRB = centerOfGravity.GetComponent<Rigidbody2D>();
    }

    //if the shears pass over the flower quick enough, it cuts it
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Shears") && shears.IsSlicing && !cut)
        {
            // Collision with shears
            rb.gravityScale = gravScale;
            rb.AddForce(Vector2.up * popForce);
            cut = true;
        }
    }
}
