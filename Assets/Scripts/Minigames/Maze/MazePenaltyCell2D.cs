using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A gentle exploration penalty: return the player to the start point without ending the game.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public sealed class MazePenaltyCell2D : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private UnityEvent onPlayerReturned;

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
