using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Events;
using System.Transactions;
using NUnit.Framework.Constraints;
using Yarn.Unity;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Interactable : MonoBehaviour
{
    public bool fading;
    public bool finished = false;
    public bool zoom;
    public bool hasDialogue;

    public string startNode;

    public float x;
    public float y;
    public float z;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = new(x, y, z);
    }

    public void Zoom()
    {
        if (transform.position.x > 0 || transform.position.y > 0)
        {
            transform.position += new Vector3(transform.position.x + -transform.position.x * 0.2f,
                transform.position.y + -transform.position.y * 0.2f, transform.position.x);
        }

    }

    private void Update()
    {
        /*
        if (fading)
        {
            float tempColor = this.GetComponent<SpriteRenderer>().color.a - 0.005f;
            if(tempColor <= 0)
            {
                Debug.Log("AERH");
                this.gameObject.SetActive(false);
               // interactmanager.endInteractableMinigame();
            }
            this.GetComponent<SpriteRenderer>().color = new Color(this.GetComponent<SpriteRenderer>().color.r,
                this.GetComponent<SpriteRenderer>().color.g, this.GetComponent<SpriteRenderer>().color.g, tempColor);
        }
        */
    }
        

}
