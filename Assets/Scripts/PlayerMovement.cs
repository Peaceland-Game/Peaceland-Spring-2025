using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerMovement : MonoBehaviour
{
    int screenWidth;
    //the spline container
    [SerializeField] SplineContainer splineContainer;
    //the spline within the spline container
    Spline spline;
    //the distance percentage of each knot on the spline
    public float[] knotDistancePercentages;

    //speed values
    [SerializeField] float walkSpeed;
    [SerializeField] float ladderSpeed;
    [SerializeField] float easeInAndOutSpeed;

    //the speed the player has this frame
    float currentSpeed = 0f;
    //the speed the player is trying to get to
    float currentDesiredSpeed;
    //the current distance percentage of the player
    float distancePercentage = 0f;
    float splineLength;

    private void Start()
    {
        //assign variables
        screenWidth = Screen.width;
        spline = splineContainer[0];
        splineLength = splineContainer.CalculateLength();
        currentDesiredSpeed = walkSpeed;

        //get the distance percentages for each knot
        float currentDistance = 0f;
        knotDistancePercentages = new float[spline.Count];
        knotDistancePercentages[0] = 0f;
        for (int i = 1; i < spline.Count; i++)
        {
            currentDistance += Vector3.Distance(spline[i - 1].Position, spline[i].Position);
            knotDistancePercentages[i] = currentDistance / splineLength;
        }
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
                    MoveAlongPath(-easeInAndOutSpeed);
                }
            }
            //if the cursor is on the right side of the screen
            else
            {
                //if the player is not at the end of the path
                if (distancePercentage < 1)
                {
                    //go right
                    MoveAlongPath(easeInAndOutSpeed);
                }
            }
        }
        //if mouse is not held down
        else
        {
            //slow to a stop if not at start or end
            if (currentSpeed < -0.1f)
            {
                MoveAlongPath(easeInAndOutSpeed);
            }
            else if (currentSpeed > 0.1f)
            {
                MoveAlongPath(-easeInAndOutSpeed);
            }
            else
            {
                currentSpeed = 0;
            }

            //immedietly stop if at start or end
            if (distancePercentage <= 0 || distancePercentage >= 1)
            {
                currentSpeed = 0;
            }
        }

        //example of going slower in between 2 knots of the path
        //when the player is between knots 1 and 2 (a ladder), go slower
        if (distancePercentage > knotDistancePercentages[2] && distancePercentage < knotDistancePercentages[3])
        {
            currentDesiredSpeed = ladderSpeed / 2;
        }
        else
        {
            currentDesiredSpeed = walkSpeed;
        }
    }

    /// <summary>
    /// moves the player along the given spline in a direction
    /// </summary>
    /// <param name="speedChange"></param>
    private void MoveAlongPath(float speedChange)
    {
        //increase/decrease speed if it is not at max or min
        currentSpeed += speedChange * Time.deltaTime;
        if (currentSpeed <= -currentDesiredSpeed)
        {
            currentSpeed = -currentDesiredSpeed;
        }
        else if (currentSpeed >= currentDesiredSpeed)
        {
            currentSpeed = currentDesiredSpeed;
        }

        //update the distance percentage and move the player on teh twine to that position
        distancePercentage += currentSpeed * Time.deltaTime / splineLength;

        Vector3 currentPosition = spline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;
    }
}
