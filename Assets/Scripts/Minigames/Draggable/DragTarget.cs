using UnityEngine;

public class DragTarget : MonoBehaviour
{
    FlowerType typeOfFlower;

    /// <summary>
    /// Type of flower allowed to be dragged on this target
    /// </summary>
    public FlowerType TypeOfFlower { get { return typeOfFlower; } }

    /// <summary>
    /// Is there a flower snapped to this target?
    /// </summary>
    [SerializeField]
    public bool isSnapped = false;

    public void Constructor(FlowerType _typeOfFlower)
    {
        typeOfFlower = _typeOfFlower;

        //set the sprite
        GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetFlowerTopSprite(_typeOfFlower);
    }
}
