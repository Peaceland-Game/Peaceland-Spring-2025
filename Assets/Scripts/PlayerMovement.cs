using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] SplineContainer spline;
    [SerializeField] float speed;
    
    int screenWidth;

    float distancePercentage = 0f;
    float splineLength;

    private void Start()
    {
        screenWidth = Screen.width;
        splineLength = spline.CalculateLength();
    }

    // Update is called once per frame
    void Update()
    {
        //if the mouse is held down
        if (Input.GetMouseButton(0))
        {
            //if the cursor is on the left side of the screen
            if (Input.mousePosition.x < screenWidth / 2)
            {
                //if the player is not at the start of the path
                if (distancePercentage > 0)
                {
                    //go left
                    MoveAlongPath(-1);
                }
            }
            //if the cursor is on the right side of the screen
            else
            {
                //if the player is not at the end of the path
                if (distancePercentage < 1)
                {
                    //go right
                    MoveAlongPath(1);
                }
            }
        }
    }

    /// <summary>
    /// moves the player along the given spline in a direction
    /// </summary>
    /// <param name="direction"></param>
    private void MoveAlongPath(int direction)
    {
        distancePercentage += direction * speed * Time.deltaTime / splineLength;

        Vector3 currentPosition = spline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        Vector3 nextPosition = spline.EvaluatePosition(distancePercentage + 0.05f);
        //Vector3 direction = nextPosition - currentPosition;
        //transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }
}
