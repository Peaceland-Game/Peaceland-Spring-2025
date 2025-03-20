using UnityEngine;

/// <summary>
/// A sprite that can be cut dynamically using a DynamicCutter.
/// Stores the world position for each of the pixels within the object's sprite.
/// IMPORTANT: A DYNAMIC SPRITE MUST HAVE ITS PIVOT SET TO THE TOP LEFT
/// </summary>
public class DynamicSprite : MonoBehaviour
{
    // Fields
    Sprite sprite;
    Rect rect; // rect for the sprite
    Texture2D texture; // The texture of the sprite
    Color[,] pixels;
    Vector2[,] pixelWorldPositions;
    float PPU; // Pixels per unit of the sprite

    // Properties


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get the texture of the object the script is attached to
        sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        texture = sprite.texture;
        rect = sprite.rect;
        PPU = sprite.pixelsPerUnit;

        // Call static class method to make a 2D array from a 1D array
        // This makes it easier to get the pixel world positions mathematically
        pixels = DynamicCutHelper.Make2DArrayFrom1D(texture.GetPixels(), (int)rect.width, (int)rect.height);
        pixelWorldPositions = new Vector2[(int)rect.height, (int)rect.width];
        GetPixelWorldPositions();
    }

    /// <summary>
    /// Takes each individual pixel and stores its world position into an array
    /// </summary>
    void GetPixelWorldPositions()
    {
        // Get the transform X and Y scale of the obj
        float xScale = gameObject.transform.localScale.x;
        float yScale = gameObject.transform.localScale.y;
        
        // Loop through the array of pixels and get
        // the world position of each pixel within the sprite
        for(int col = 0;  col < pixels.GetLength(0); col++)
        {
            for(int row = 0; row < pixels.GetLength(1); row++)
            {
                pixelWorldPositions[col, row] = CalculateWorldPosOfPixel(col, row, xScale, yScale);
            }
        }
        int x = 0;
    }
    
    /// <summary>
    /// Calculates the world positioin of a pixel within a sprite array
    /// </summary>
    /// <param name="col">The x coordinate of the pixel
    /// in the pixel array</param>
    /// <param name="row">The y coordinate of the pixel
    /// in the pixel array</param>
    /// <param name="xScale">The gameObject's scale
    /// </param>
    /// <returns>A vector that corresponds to the pixel's world position</returns>
    Vector2 CalculateWorldPosOfPixel(int col, int row, float xScale, float yScale)
    {
        float xCoord = col * (1 / PPU) + gameObject.transform.position.x * xScale;
        float yCoord = row * (1 / PPU) + gameObject.transform.position.y * yScale;

        return new Vector2(xCoord, yCoord);
    }
}
