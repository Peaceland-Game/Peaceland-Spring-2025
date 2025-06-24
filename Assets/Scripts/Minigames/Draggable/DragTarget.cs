using UnityEngine;

public class DragTarget : MonoBehaviour
{
    OrderObjectType typeOfObject;

    /// <summary>
    /// Type of object allowed to be dragged on this target
    /// </summary>
    public OrderObjectType TypeOfObject { get { return typeOfObject; } }

    /// <summary>
    /// Is there an object snapped to this target?
    /// </summary>
    [SerializeField]
    public bool isSnapped = false;

    public void Constructor(OrderObjectType _typeOfObject)
    {
        typeOfObject = _typeOfObject;

        //set the sprite
        GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetFlowerTopSprite(_typeOfObject);
    }
}
