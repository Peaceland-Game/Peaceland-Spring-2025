using UnityEngine;

public class Target : MonoBehaviour
{
    int typeOfFlower;

    public int TypeOfFlower { get { return typeOfFlower; } }

    public void Constructor(int _typeOfFlower, Sprite flowerSprite)
    {
        typeOfFlower = _typeOfFlower;

        //set the sprite
        GetComponent<SpriteRenderer>().sprite = flowerSprite;
    }
}
