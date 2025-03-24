using UnityEditor;
using UnityEngine;

/// <summary>
/// A sprite that can be cut dynamically using a DynamicCutter.
/// Stores the world position for each of the pixels within the object's sprite.
/// IMPORTANT: A DYNAMIC SPRITE MUST HAVE ITS PIVOT SET TO THE TOP LEFT FOR CALCULATIONS TO WORK PROPERLY
/// </summary>
public class DynamicSprite : MonoBehaviour
{
    // Enum to say if this sprite is the "top" sprite or the "bottom" sprite
    enum Type
    {
        TOP = 0,
        BOTTOM = 1
    }

    [SerializeField]
    Type type;

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

        // Add this sprite to respond to dynamic cutter events
        DynamicCutter.onCutCreated += SplitSprite;

        // Call static class method to make a 2D array from a 1D array
        // This makes it easier to get the pixel world positions mathematically
        Debug.Log(texture.GetPixels().Length);
        pixels = DynamicCutHelper.Make2DArrayFrom1D(texture.GetPixels(), texture.width, texture.height);
        pixelWorldPositions = new Vector2[texture.height, texture.width];

        // Populate the pixel position array
        GetPixelWorldPositions();
    }

    /// <summary>
    /// Takes each individual pixel and stores a rough estimate of its world position into an array
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
                //Debug.Log($"Pixel Col: {col}, Pixel Row: {row}, " +
                //    $"Pixel Coordinates: X: {pixelWorldPositions[col, row].x} Y: {pixelWorldPositions[col, row].y}");
            }
        }
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

    /// <summary>
    /// Takes in the DynamicCutter's cut line and sets
    /// pixels below the line to transparent if this sprite is TOP,
    /// pixels above the line will be set to transparent if this sprite is BOTTOM
    /// </summary>
    /// <param name="cutLine">The line made by the DynamicCutter</param>
    void SplitSprite(Vector2 lineStart, Vector2 lineEnd)
    {
        switch(type)
        {
            case Type.TOP:
                SplitSpriteTop(lineStart, lineEnd);
                transform.position.Set(-5.0f, 0, 0);
                Debug.Log("Top moved");
                break;
            case Type.BOTTOM:
                SplitSpriteBottom(lineStart, lineEnd);
                transform.position.Set(5.0f, 0, 0);
                Debug.Log("Bottom moved");
                break;
            default:
                Debug.Log("ERROR: DynamicSprite type hasn't been set!");
                return;
        }
    }

    void SplitSpriteTop(Vector2 lineStart, Vector2 lineEnd)
    {
        // Loop through pixel positions
        for(int col = 0; col < pixelWorldPositions.GetLength(0); col++)
        {
            for(int row = 0; row < pixelWorldPositions.GetLength(1); row++)
            {
                // If the distance is negative (below the line)
                // we set the corresponding pixel to transparent
                if (pixelWorldPositions[col, row].y < lineStart.y)
                {
                    pixels[col, row].a = 0;
                }
            }
        }
        // We convert back to a 1D array because Unity sucks and doesn't give pixels in a 2D array by default
        Color[] adjPixels = DynamicCutHelper.Make1DArrayFrom2D(pixels);

        // Apply all changes we made to the sprite
        texture.SetPixels(adjPixels);
        texture.Apply();
    }

    void SplitSpriteBottom(Vector2 lineStart, Vector2 lineEnd)
    {
        // Loop through pixel positions
        for (int col = 0; col < pixelWorldPositions.GetLength(0); col++)
        {
            for (int row = 0; row < pixelWorldPositions.GetLength(1); row++)
            {
                // If the distance is positive (above the line)
                // we set the corresponding pixel to transparent
                if(pixelWorldPositions[col, row].y > lineStart.y)
                {
                    pixels[col, row].a = 0;
                }
            }
        }
        // We convert back to a 1D array because Unity sucks and doesn't give pixels in a 2D array by default
        Color[] adjPixels = DynamicCutHelper.Make1DArrayFrom2D(pixels);

        // Apply all changes we made to the sprite
        texture.SetPixels(adjPixels);
        texture.Apply();
    }

    // Need to remove events
    public void OnDestroy()
    {
        DynamicCutter.onCutCreated -= SplitSprite;
    }
}
