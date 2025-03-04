using System.Drawing.Text;
using UnityEngine;

public class CutLogic : MonoBehaviour
{

    /// <summary>
    /// The rigidbody that will physically be "cut off" by the player
    /// </summary>
    [SerializeField] Rigidbody2D cutRB;
    /// <summary>
    /// gravity scaling for piece once it is cut
    /// </summary>
    [SerializeField] float gravScale;
    /// <summary>
    /// the magnitude of the force applied to the piece when it is cut
    /// </summary>
    [SerializeField] float popForce;
    /// <summary>
    /// the distance the player must drag the shears for a new point to be drawn.
    /// </summary>
    [SerializeField] float cutDistance;
    /// <summary>
    /// the dotted line that the player tries to cut on
    /// </summary>
    [SerializeField] LineRenderer guideLine;
    /// <summary>
    /// the line that is drawn as the player cuts
    /// </summary>
    [SerializeField] LineRenderer cutLine;
    /// <summary>
    /// the max average distance for a perfect score
    /// </summary>
    [SerializeField] float maxGreatDistance;
    /// <summary>
    /// the max average distance for an okay score
    /// </summary>
    [SerializeField] float maxOkayDistance;

    private bool cut;

    public bool Cut { get => cut; }

    private void Start()
    {
        cut = false;
    }

    

    private void OnTriggerStay2D(Collider2D collider)
    {
        //if the shears are over the area to be cut
        if (!Cut && collider.CompareTag("Shears"))
        {
            //if the line has not been started yet
            if (cutLine.positionCount == 0)
            {
                //add a new point to the cut line
                cutLine.positionCount++;
                cutLine.SetPosition(cutLine.positionCount - 1, collider.gameObject.transform.position);
            }
            //if the shears are far enough away from the last drawn point
            else if (Vector2.Distance(cutLine.GetPosition(cutLine.positionCount - 1), collider.gameObject.transform.position) > cutDistance)
            {
                //add a new point to the cut line
                cutLine.positionCount++;
                cutLine.SetPosition(cutLine.positionCount - 1, collider.gameObject.transform.position);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        //if the shears exit the area that is getting cut
        if (!Cut && collider.CompareTag("Shears"))
        {
            CalculateCutScore();
        }

        // *** ACTUAL CUTTING ** //
        if (collider.CompareTag("Shears") && collider.GetComponent<GrabAndSwipe>().IsSlicing && !Cut)
        {
            CutObj();
        }
    }

    private void CutObj()
    {
        // Collision with shears
        cutRB.gravityScale = gravScale;
        cutRB.AddForce(Vector2.up * popForce);
        cut = true;

        CutManager.CutMade();
        
    }

    private void CalculateCutScore()
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

        //calculate the score
        if (averageDistance <= maxGreatDistance)
        {
            Debug.Log("Great Work!");
        }
        else if (averageDistance <= maxOkayDistance)
        {
            Debug.Log("You did alright!");
        }
        else
        {
            Debug.Log("You can do better...");
        }

        //reset cut line
        cutLine.positionCount = 0;
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
