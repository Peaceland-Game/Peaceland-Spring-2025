using System.Collections;
using System.Drawing.Text;
using UnityEngine;

public class CutLogic : MonoBehaviour
{

    /// <summary>
    /// The rigidbody that will physically be "cut off" by the player
    /// </summary>
    [SerializeField] Rigidbody2D cutRB;
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
    /// the gameObject that will be cut off
    /// </summary>
    [SerializeField] GameObject cutPiece;
    /// <summary>
    /// the max average distance for a perfect score
    /// </summary>
    [SerializeField] float maxGreatDistance;
    /// <summary>
    /// the max average distance for an okay score
    /// </summary>
    [SerializeField] float maxOkayDistance;

    private bool cut;

    Stem stem;

    public bool Cut { get => cut; }

    private void Start()
    {
        cut = false;
    }

    public void AssignValues(LineRenderer _cutLine, Stem _stem)
    {
        cutLine = _cutLine;
        stem = _stem;
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        //if the shears are over the area to be cut
        if (!Cut && collider.CompareTag("Shears"))
        {
            //if the line has not been started yet
            if (cutLine.positionCount == 0 || Vector2.Distance(cutLine.GetPosition(cutLine.positionCount - 1), collider.gameObject.transform.position) > cutDistance)
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
        if (collider.CompareTag("Shears") && !Cut)
        {
            CutObj();
        }
    }

    private void CutObj()
    {
        //disable hitbox
        GetComponent<BoxCollider2D>().enabled = false;

        // destory guide line after doing any effects
        guideLine.GetComponent<GuideLine>().DestroySequence();

        cut = true;

        //move object off of the screen
        StartCoroutine(cutPiece.GetComponent<CutPiece>().MoveSnippedObject(1, 1, 2f));

        //transition to next cut or next flower
        if (stem != null) { stem.CutMade(transform.parent.gameObject); }
        else { StartCoroutine(CutManager.AllCutsMade()); }
    }

    private void CalculateCutScore()
    {
        float averageDistance = 0;
        //gets the distance of each point on the drawn line
        Vector3 guideLinePos1 = guideLine.GetPosition(0);
        Vector3 guideLinePos2 = guideLine.GetPosition(1);
        for (int i = 0; i < cutLine.positionCount; i++)
        {
            averageDistance += LinePointDistance(guideLinePos1, guideLinePos2, cutLine.GetPosition(i));
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
    private float LinePointDistance(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        Vector2 lineDirection = lineEnd - lineStart;
        Vector2 pointToLineStart = point - lineStart;
        return Vector3.Cross(pointToLineStart, lineDirection).magnitude / lineDirection.magnitude;
    }
}
