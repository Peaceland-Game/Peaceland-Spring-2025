using UnityEngine;

/// <summary>
/// Explicit composition entry point for the Maze prototype.
/// </summary>
[DefaultExecutionOrder(-10000)]
public sealed class MazeGameBootstrap : MonoBehaviour
{
    [SerializeField] private MazePlayerController2D player;
    [SerializeField] private MazeCameraController2D cameraController;
    [SerializeField] private Transform startPoint;

    private void Awake()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<MazePlayerController2D>();
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<MazeCameraController2D>();
        }

        if (player != null && startPoint != null)
        {
            player.SetStartPoint(startPoint);
            player.ResetToStart();
        }

        if (cameraController != null && player != null)
        {
            cameraController.SetTarget(player.transform);
        }
    }

    public void ResetMaze()
    {
        if (player != null)
        {
            player.ResetToStart();
        }
    }
}
