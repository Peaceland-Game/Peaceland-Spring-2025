using UnityEngine;

public class Checklist : MonoBehaviour
{
    /// <summary>
    /// The x-coordinate where the checklist should start
    /// </summary>
    private double checklistStartPoint;

    /// <summary>
    /// The x-coordinate for where the checklist should stop moving
    /// </summary>
    private double checklistStopPoint = -6.74;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        checklistStartPoint = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// When the player clicks on the checklsit, move it to the side of the screen
    /// </summary>
    private void OnMouseDown()
    {
        if (transform.position.x < checklistStopPoint)
        {
            transform.position += new Vector3(0.5f, 0, 0);

            if (transform.position.x >= checklistStopPoint)
            {
                transform.position = new Vector3((float)checklistStopPoint, 0, 0);
            }
        }

        else if (transform.position.x > checklistStartPoint)
        {
            transform.position -= new Vector3(0.5f, 0, 0);

            if (transform.position.x <= checklistStartPoint)
            {
                transform.position = new Vector3((float)checklistStartPoint, 0, 0);
            }
        }
    }
}
