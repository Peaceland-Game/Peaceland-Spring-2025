using UnityEngine;

public class FlowerLogic : MonoBehaviour
{
    [SerializeField] GrabAndSwipe shears;

    //if the shears pass over the flower quick enough, it cuts it
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Shears") && shears.IsSlicing)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}
