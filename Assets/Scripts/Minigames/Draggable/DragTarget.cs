using UnityEngine;

public class DragTarget : MonoBehaviour
{
    
    /// <summary>
    /// Holds a reference to the target type object.
    /// </summary>
    public object targetType;

    /// <summary>
    /// Is there a draggable object snapped to this target?
    /// </summary>
    [SerializeField]
    public bool isSnapped = false;

    [SerializeField]
    public Draggable currentSnappedPiece = null;

    [SerializeField]
    private bool preservePrefabSprite = false;

    public void Constructor<T>(T _dataType, Sprite sprite = null)
    {
        targetType = _dataType;

        if (!preservePrefabSprite && sprite != null)
        {
            //set the sprite
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

    }

    /// <summary>
    /// Snaps a new draggable piece into this target slot. If another piece is already snapped,
    /// it removes and displaces the previous piece back to its starting word bank position.
    /// </summary>
    public void SnapPiece(Draggable newPiece)
    {
        if (currentSnappedPiece != null && currentSnappedPiece != newPiece)
        {
            // Return old piece back to its original slot in the word bank
            currentSnappedPiece.ReturnToStartPos();
        }

        currentSnappedPiece = newPiece;
        isSnapped = (newPiece != null);
    }

    /// <summary>
    /// Unsnaps the current piece from this target slot.
    /// </summary>
    public void UnsnapPiece(Draggable piece)
    {
        if (currentSnappedPiece == piece)
        {
            currentSnappedPiece = null;
            isSnapped = false;
        }
    }

    /// <summary>
    /// Determines whether the specified type matches the target type.
    /// </summary>
    /// <param name="incomingType">The type to compare with the target type.</param>
    /// <returns>True if the incoming type equals the target type; otherwise, false.</returns>
    public bool CanSnap(object incomingType)
    {
        if (targetType == null || incomingType == null) return true;
        if (Equals(targetType, incomingType)) return true;
        return string.Equals(targetType.ToString(), incomingType.ToString(), System.StringComparison.OrdinalIgnoreCase);
    }
}
