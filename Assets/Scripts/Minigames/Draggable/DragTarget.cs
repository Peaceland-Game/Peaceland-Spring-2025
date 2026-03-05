using UnityEngine;

public class DragTarget : MonoBehaviour
{
    
    /// <summary>
    /// Holds a reference to the target type object.
    /// </summary>
    private object targetType;

    /// <summary>
    /// Is there a draggable object snapped to this target?
    /// </summary>
    [SerializeField]
    public bool isSnapped = false;

    public void Constructor<T>(T _dataType, Sprite sprite = null)
    {
        targetType = _dataType;

        if (sprite != null)
        {
            //set the sprite
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

    }

    /// <summary>
    /// Determines whether the specified type matches the target type.
    /// </summary>
    /// <param name="incomingType">The type to compare with the target type.</param>
    /// <returns>True if the incoming type equals the target type; otherwise, false.</returns>
    public bool CanSnap(object incomingType)
    {
        return Equals(targetType, incomingType);
    }
}
