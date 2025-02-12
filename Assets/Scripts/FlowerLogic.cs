using UnityEngine;

public class FlowerLogic : MonoBehaviour
{
    [SerializeField] GrabAndSwipe shears;
    [SerializeField] float gravScale;
    [SerializeField] float popForce;
    [SerializeField] GameObject centerOfGravity;
    [SerializeField] float pointTime;
    [SerializeField] LineRenderer guideLine;
    [SerializeField] LineRenderer cutLine;
    float timer = 0;


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

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Shears"))
        {
            timer += Time.deltaTime;
            if (timer >= pointTime)
            {
                timer = 0;
                cutLine.positionCount++;
                cutLine.SetPosition(cutLine.positionCount - 1, collider.gameObject.transform.position);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Shears"))
        {
            float averageDistance = 0;
            //gets the distance of each point on the drawn line
            for (int i = 0; i < cutLine.positionCount; i++)
            {
                averageDistance += LinePointDistance(guideLine.GetPosition(0), guideLine.GetPosition(1), cutLine.GetPosition(i));
            }
            //calculate the average
            averageDistance /= cutLine.positionCount;
            Debug.Log("Average distance: " + averageDistance);

            //reset cut line
            cutLine.positionCount = 0;
        }
    }

    /// <summary>
    /// returns the distance between a point and a line
    /// </summary>
    /// <param name="lineStart"></param>
    /// <param name="lineEnd"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    private float LinePointDistance(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        Vector2 lineDirection = lineEnd - lineStart;
        Vector2 pointToLineStart = point - lineStart;
        return Vector3.Cross(pointToLineStart, lineDirection).magnitude / lineDirection.magnitude;
    }

}
