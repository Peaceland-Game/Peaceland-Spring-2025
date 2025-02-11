using UnityEngine;

public class PlaceDownFlower : MonoBehaviour
{
    [SerializeField] GameObject target;

    private void OnMouseUp()
    {
        if (target.GetComponent<BoxCollider2D>().IsTouching(GetComponent<BoxCollider2D>()))
        {
            transform.position = target.transform.position;
        }
        else
        {
            transform.position = Vector3.zero; 
        }
    }
}
