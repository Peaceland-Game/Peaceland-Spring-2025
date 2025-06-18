using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Events;
using System.Transactions;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Interactable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            Debug.Log(touch.position.x + ", " + touch.position.y);
        }
    }

    public void OnMouseDown()
    {
        Debug.Log("mouse press");
    }

    public void OnTap(InputAction.CallbackContext context)
    {
        Debug.Log("on touch!");
        if (!isActiveAndEnabled) return;

        if(context.phase == InputActionPhase.Started)
        {
            Debug.Log("Clicked on!!!!");
        }

    }

    private void Zoom()
    {
        if (transform.position.x > 0 || transform.position.y > 0)
        {
            transform.position += new Vector3(transform.position.x + -transform.position.x * 0.2f,
                transform.position.y + -transform.position.y * 0.2f, transform.position.x);
        }

    }

}
