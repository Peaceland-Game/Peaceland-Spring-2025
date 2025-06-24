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
        Debug.Log("TEST AHHH");
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
