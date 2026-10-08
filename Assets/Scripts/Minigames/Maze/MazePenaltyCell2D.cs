using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A gentle exploration penalty: return the player to the start point without ending the game.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public sealed class MazePenaltyCell2D : MonoBehaviour
{
    [Tooltip("Where the player is sent back to. Required.")]
    [SerializeField] private Transform startPoint;
    [Tooltip("Runs after the player is sent back.")]
    [SerializeField] private UnityEvent onPlayerReturned;

    private void Start()
    {
        if (startPoint == null)
        {
            Debug.LogWarning(name + ": Start Point is empty, so stepping here does nothing.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MazePlayerController2D player = other.GetComponentInParent<MazePlayerController2D>();
        if (player == null)
        {
            return;
        }

        if (startPoint != null)
        {
            player.SetStartPoint(startPoint);
            player.ResetToStart();
        }

        onPlayerReturned?.Invoke();
    }
}
