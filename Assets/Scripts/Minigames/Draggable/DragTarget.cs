using UnityEngine;

public class DragTarget : MonoBehaviour
{
    FlowerType typeOfFlower;

    public FlowerType TypeOfFlower { get { return typeOfFlower; } }

    [SerializeField]
    public bool isSnapped = false;

    public void Constructor(FlowerType _typeOfFlower)
    {
        typeOfFlower = _typeOfFlower;

        //set the sprite
        GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetFlowerSprite(_typeOfFlower);
    }
}
