using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

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
        EnsureEventSystem();

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

    /// <summary>
    /// The game's pause menu survives scene loads and selects a button on every scene it
    /// enters; without an EventSystem that throws once per frame. The maze does its own
    /// input, so it never asked for one - it needs one for the menu's sake.
    /// </summary>
    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null || FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    public void ResetMaze()
    {
        if (player != null)
        {
            player.ResetToStart();
        }
    }
}
