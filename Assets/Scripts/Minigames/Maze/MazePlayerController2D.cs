using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Grid-snapped, click-to-step player movement for the Maze prototype.
/// The player moves one cardinal cell per click so exploration remains deliberate.
/// </summary>
public sealed class MazePlayerController2D : MonoBehaviour
{
    [Header("Grid movement")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float moveDuration = 0.16f;
    [SerializeField] private Vector2 collisionSize = new Vector2(0.55f, 0.55f);
    [SerializeField] private LayerMask blockingMask;

    [Header("Input")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private bool acceptPointerInput = true;

    [Header("Reset")]
    [SerializeField] private Transform startPoint;

    private Coroutine moveRoutine;
    private bool movementEnabled = true;
    private Vector2 facing = Vector2.down;

    public bool IsMoving => moveRoutine != null;
    public bool MovementEnabled => movementEnabled;
    public Vector2 Facing => facing;

    private void Awake()
    {
        int portableBlockingMask = LayerMask.GetMask("MazePhysical", "Wall");
        if (portableBlockingMask != 0)
        {
            blockingMask = portableBlockingMask;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (blockingMask.value == 0)
        {
            blockingMask = LayerMask.GetMask("MazePhysical", "Wall");
        }

        if (startPoint != null)
        {
            ResetToStart();
        }
    }

    private void Update()
    {
        if (!movementEnabled || IsMoving || !acceptPointerInput || Pointer.current == null)
        {
            return;
        }

        if (Pointer.current.press.wasPressedThisFrame)
        {
            RequestStep(Pointer.current.position.ReadValue());
        }
    }

    public void SetStartPoint(Transform newStartPoint)
    {
        startPoint = newStartPoint;
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
    }

    public void ResetToStart()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        if (startPoint != null)
        {
            transform.position = startPoint.position;
        }

        movementEnabled = true;
        facing = Vector2.down;
        ApplyFacingRotation();
    }

    public bool RequestStep(Vector2 screenPosition)
    {
        if (!movementEnabled || IsMoving)
        {
            return false;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (worldCamera == null)
        {
            return false;
        }

        float distance = Mathf.Abs(worldCamera.transform.position.z - transform.position.z);
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, distance);
        Vector3 worldPoint = worldCamera.ScreenToWorldPoint(screenPoint);

        if (!TryBuildPath(transform.position, worldPoint, out System.Collections.Generic.List<Vector2> path) || path.Count == 0)
        {
            return false;
        }

        moveRoutine = StartCoroutine(MoveAlongPath(path));
        return true;
    }

    private IEnumerator MoveToCell(Vector2 target)
    {
        Vector3 start = transform.position;
        Vector3 end = new Vector3(target.x, target.y, start.z);
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, moveDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        transform.position = end;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, collisionSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)facing * cellSize);
    }


    private IEnumerator MoveAlongPath(System.Collections.Generic.List<Vector2> path)
    {
        foreach (Vector2 target in path)
        {
            Vector2 delta = target - (Vector2)transform.position;
            if (delta.sqrMagnitude <= 0.001f)
            {
                continue;
            }

            facing = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? new Vector2(Mathf.Sign(delta.x), 0f)
                : new Vector2(0f, Mathf.Sign(delta.y));
            ApplyFacingRotation();
            yield return MoveToCell(target);
        }

        moveRoutine = null;
    }

    private void ApplyFacingRotation()
    {
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
        Transform visual = transform.Find("Visual");
        if (visual != null)
        {
            visual.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Transform visionCone = transform.Find("VisionCone");
        if (visionCone != null)
        {
            visionCone.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }


    private bool TryBuildPath(Vector2 startWorld, Vector2 targetWorld, out System.Collections.Generic.List<Vector2> path)
    {
        return MazeGridPathfinder2D.TryBuildPath(
            startWorld,
            targetWorld,
            cellSize,
            collisionSize,
            blockingMask,
            out path);
    }
}
