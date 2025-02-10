using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GrabAndSwipe : MonoBehaviour
{
    //the speed the shears need to be moving in order to slice
    [SerializeField] float sliceSpeed;
    bool isSlicing;
    Vector2 previousMousePos;
    SpriteRenderer spriteRenderer;

    public bool IsSlicing { get { return isSlicing; } }

    private void Start()
    {
        isSlicing = false;
        previousMousePos = Vector2.zero;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDrag()
    {
        transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);
        if (Vector2.Distance(previousMousePos, transform.position) / Time.deltaTime >= sliceSpeed)
        {
            isSlicing = true;
            spriteRenderer.color = Color.red;
        }
        else
        {
            isSlicing = false;
            spriteRenderer.color = Color.white;
        }
        previousMousePos = transform.position;
    }
}
