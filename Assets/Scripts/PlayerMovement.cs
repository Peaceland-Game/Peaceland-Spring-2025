using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerMovement : MonoBehaviour
{
    int screenWidth;

    [SerializeField] SplineContainer spline;
    [SerializeField] float maxSpeed;
    [SerializeField] float easeInAndOutSpeed;

    float currentSpeed = 0f;
    float distancePercentage = 0f;
    float splineLength;
    Vector3 tangent;

    private void Start()
    {
        screenWidth = Screen.width;
        splineLength = spline.CalculateLength();
        tangent = spline.EvaluateTangent(1);
        Debug.Log(tangent);
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
    }

    /// <summary>
    /// moves the player along the given spline in a direction
    /// </summary>
    /// <param name="speedChange"></param>
    private void MoveAlongPath(float speedChange)
    {
        //increase/decrease speed if it is not at max or min
        currentSpeed += speedChange * Time.deltaTime;
        if (currentSpeed <= -maxSpeed)
        {
            currentSpeed = -maxSpeed;
        }
        else if (currentSpeed >= maxSpeed)
        {
            currentSpeed = maxSpeed;
        }

        //update the distance percentage and move the player on teh twine to that position
        distancePercentage += currentSpeed * Time.deltaTime / splineLength;

        Vector3 currentPosition = spline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        //if we want the player's rotation to change, use this
        //Vector3 nextPosition = spline.EvaluatePosition(distancePercentage + 0.05f);
        //Vector3 direction = nextPosition - currentPosition;
        //transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }
}
