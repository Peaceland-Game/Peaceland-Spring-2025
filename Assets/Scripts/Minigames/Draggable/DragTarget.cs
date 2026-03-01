using UnityEngine;

public class DragTarget : MonoBehaviour
{
    

    private object targetType;

    /// <summary>
    /// Is there a flower snapped to this target?
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

    public bool CanSnap(object incomingType)
    {
        return Equals(targetType, incomingType);
    }
}
