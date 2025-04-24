using System.Collections;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using UnityEngine;

public class CutLogic : MonoBehaviour
{
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

    private Vector2 cutStart;
    private Vector2 cutEnd;

    // Delegates and events for CuttableFlower listeners
    public delegate void OnCut(Vector2 cutStart, Vector2 cutEnd);
    public static event OnCut onCut;

    Stem stem;

    public bool Cut { get => cut; }


    private void OnDrawGizmos()
    {
    }
    private void Start()
    {
        cut = false;
    }

    /// <summary>
    /// Initializes cutLine and stem values
    /// </summary>
    /// <param name="_cutLine">CutLine to store</param>
    /// <param name="_stem">Stem to store</param>
    public void AssignValues(LineRenderer _cutLine, Stem _stem)
    {
        cutLine = _cutLine;
        stem = _stem;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        //if the shears are over the area to be cut
        if (!Cut && collider.CompareTag("Shears"))
        {
            // set the start of the cut 
            cutStart = collider.transform.position;
        }
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
            cutEnd = collider.transform.position;

            CalculateCutScore();

            Debug.Log(this.gameObject.transform.parent.name);

            // Send the actual coordinates of the cut to CuttableFlower
            // to dynamically divide the sprites
            // Only will happen with CuttableFlower
            if (this.gameObject.transform.parent.CompareTag("CuttableFlower"))
            {
                Debug.Log("OnCut invoked");
                onCut.Invoke(cutStart, cutEnd);
            }
            CutObj();
        }
    }

    /// <summary>
    /// Initializes cut sequence of the CutLogic object.
    /// Disables the hitbox, sets the state of the object
    /// to cut, and then animates the cutPiece off screen.
    /// Transitions to next cut after the couroutine is finished.
    /// </summary>
    private void CutObj()
    {
        //disable hitbox
        GetComponent<BoxCollider2D>().enabled = false;

        // destory guide line after doing any effects
        guideLine.GetComponent<GuideLine>().DestroySequence();

        cut = true;

        //move object off of the screen
        StartCoroutine(cutPiece?.GetComponent<CutPiece>().MoveSnippedObject(1, 1, 2f));

        //transition to next cut or next flower
        if (stem != null) { stem.CutMade(transform.parent.gameObject); }
        else { StartCoroutine(CutManager.AllCutsMade()); }
    }

    /// <summary>
    /// Uses the point-line distance formula to calculate an
    /// accuracy score based on how well the user stayed within
    /// the hitbox of the CutLogic object.
    /// </summary>
    private void CalculateCutScore()
    {
        float averageDistance = 0;
        //gets the distance of each point on the drawn line
        Vector3 guideLinePos1 = guideLine.GetPosition(0) + guideLine.transform.position;
        Vector3 guideLinePos2 = guideLine.GetPosition(1) + guideLine.transform.position;
        for (int i = 0; i < cutLine.positionCount; i++)
        {
            averageDistance += LinePointDistance(guideLinePos1, guideLinePos2, cutLine.GetPosition(i));
        }
        //calculate the average
        averageDistance /= cutLine.positionCount;
        Debug.Log("Average distance: " + averageDistance);

        // Display score response
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
