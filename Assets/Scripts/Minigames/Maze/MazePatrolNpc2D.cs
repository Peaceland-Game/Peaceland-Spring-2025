using System.Collections;
using UnityEngine;

public sealed class MazePatrolNpc2D : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float moveDuration = 0.32f;
    [SerializeField] private float waitAtPoint = 0.45f;
    [SerializeField] private bool loop = true;

    private Coroutine patrolRoutine;
    private bool isPatrolling;

    private int currentIndex;

    public bool IsPatrolling => isPatrolling;

private void Start()
    {
        // Keep the gameplay root axis-aligned. Only the visual representation
        // should turn; rotating the root can flip the 2D sprite out of view.
        transform.rotation = Quaternion.identity;

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[0].position;
            currentIndex = 0;
            isPatrolling = patrolPoints.Length > 1;
            patrolRoutine = isPatrolling ? StartCoroutine(PatrolLoop()) : null;
        }
    }

private IEnumerator PatrolLoop()
    {
        while (true)
        {
            if (patrolPoints == null || patrolPoints.Length < 2)
            {
                isPatrolling = false;
                patrolRoutine = null;
                yield break;
            }

            int nextIndex = (currentIndex + 1) % patrolPoints.Length;
            Vector2 target = patrolPoints[nextIndex].position;
            if (TryBuildPath(transform.position, target, out System.Collections.Generic.List<Vector2> path))
            {
                yield return MoveAlongPath(path);
            }
            else
            {
                Debug.LogWarning($"NPC patrol target is blocked or unreachable: {target}", this);
            }

            transform.position = new Vector3(
                Mathf.Round(transform.position.x),
                Mathf.Round(transform.position.y),
                transform.position.z);
            currentIndex = nextIndex;
            yield return new WaitForSeconds(Mathf.Max(0f, waitAtPoint));

            if (!loop && currentIndex == patrolPoints.Length - 1)
            {
                isPatrolling = false;
                patrolRoutine = null;
                yield break;
            }
        }
    }

    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
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


private IEnumerator MoveAlongPath(System.Collections.Generic.List<Vector2> path)
    {
        foreach (Vector2 target in path)
        {
            Vector2 delta = target - (Vector2)transform.position;
            if (delta.sqrMagnitude <= 0.001f)
            {
                continue;
            }

            Vector2 facing = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? new Vector2(Mathf.Sign(delta.x), 0f)
                : new Vector2(0f, Mathf.Sign(delta.y));
            ApplyFacingRotation(facing);
            yield return MoveToCell(target);
        }
    }

    private void ApplyFacingRotation(Vector2 facing)
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
        const float npcCellSize = 1f;
        Vector2 collisionSize = new Vector2(0.55f, 0.55f);
        int blockingMask = LayerMask.GetMask("MazePhysical", "Wall");
        return MazeGridPathfinder2D.TryBuildPath(
            startWorld,
            targetWorld,
            npcCellSize,
            collisionSize,
            blockingMask,
            out path);
    }
}
