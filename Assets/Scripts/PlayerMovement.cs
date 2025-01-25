using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    int screenWidth;
    int screenHeight;

    Rigidbody2D rb;

    [SerializeField]
    float walkForce;
    [SerializeField]
    float maxSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        //player moves left if they press the left side of the screen, and right if they press the right side
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x < screenWidth / 2)
            {
                rb.AddForce(new Vector2(-walkForce * Time.deltaTime, 0));
            }
            else
            {
                rb.AddForce(new Vector2(walkForce * Time.deltaTime, 0));
            }
        }

        //player can not move faster than max speed
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }
}
