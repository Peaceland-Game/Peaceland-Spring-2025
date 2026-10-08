using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Walks a fixed route until it sees the player, then closes in.
/// Being seen is not instantly fatal: suspicion has to fill first, so breaking
/// line of sight is always a way out and the cone reads as a warning, not a wall.
/// </summary>
public sealed class MazePatrolNpc2D : MonoBehaviour
{
    [Header("Patrol")]
    [Tooltip("Points the NPC walks between, in order. Place them on whole-number grid cells. Two or more to move at all.")]
    [SerializeField] private Transform[] patrolPoints;
    [Tooltip("Seconds per grid step while patrolling.")]
    [SerializeField] private float moveDuration = 0.32f;
    [Tooltip("Seconds to pause at each patrol point.")]
    [SerializeField] private float waitAtPoint = 0.45f;
    [Tooltip("Go back to the first point after the last. Off: stop at the last point.")]
    [SerializeField] private bool loop = true;

    [Header("Perception")]
    [Tooltip("Found in children when empty.")]
    [SerializeField] private MazeVisionCone2D visionCone;
    [Tooltip("Found in the scene when empty.")]
    [SerializeField] private MazePlayerController2D player;
    [Tooltip("Seconds of unbroken sight before the player is caught.")]
    [SerializeField] private float suspicionToCatch = 1.2f;
    [Tooltip("Suspicion lost per second once line of sight is broken.")]
    [SerializeField] private float suspicionDecayRate = 0.8f;
    [Tooltip("How much faster the NPC steps while it is closing in.")]
    [SerializeField] private float chaseSpeedMultiplier = 1.4f;

    [Header("Events")]
    [Tooltip("Raised the moment the player enters the cone.")]
    [SerializeField] private UnityEvent onPlayerSpotted;
    [Tooltip("Raised when suspicion fills. Hook the objective reset here.")]
    [SerializeField] private UnityEvent onPlayerCaught;

    private Coroutine behaviourRoutine;
    private int currentIndex;
    private float suspicion;
    private bool wasSeeing;

    public bool IsPatrolling => behaviourRoutine != null && suspicion <= 0f;
    public bool IsChasing => suspicion > 0f;

    /// <summary>0 to 1. Drive an alert icon or the cone colour from this.</summary>
    public float Suspicion01 =>
        suspicionToCatch <= 0f ? 0f : Mathf.Clamp01(suspicion / suspicionToCatch);

    private void Start()
    {
        // Keep the gameplay root axis-aligned. Only the visual representation
        // should turn; rotating the root can flip the 2D sprite out of view.
        transform.rotation = Quaternion.identity;

        if (visionCone == null)
        {
            visionCone = GetComponentInChildren<MazeVisionCone2D>(true);
        }

        if (player == null)
        {
            player = FindFirstObjectByType<MazePlayerController2D>(FindObjectsInactive.Include);
        }

        if (patrolPoints != null && System.Array.IndexOf(patrolPoints, null) >= 0)
        {
            Debug.LogWarning(name + ": Patrol Points has empty slots; they are skipped.", this);
            patrolPoints = System.Array.FindAll(patrolPoints, point => point != null);
        }

        if (visionCone == null || player == null)
        {
            Debug.LogWarning(name + ": no vision cone child or no MazePlayerController2D in the scene, so this NPC can never catch anyone.", this);
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[0].position;
            currentIndex = 0;
        }

        behaviourRoutine = StartCoroutine(RunBehaviour());
    }

    private void Update()
    {
        if (visionCone == null || player == null)
        {
            return;
        }

        // While the player is frozen (dialogue, cutscene) they cannot dodge, so they cannot be caught either.
        bool canSee = player.MovementEnabled && visionCone.CanSee(player.transform.position);
        if (canSee)
        {
            if (!wasSeeing)
            {
                onPlayerSpotted?.Invoke();
            }

            suspicion += Time.deltaTime;
            if (suspicion >= suspicionToCatch)
            {
                Catch();
            }
        }
        else
        {
            suspicion = Mathf.Max(0f, suspicion - suspicionDecayRate * Time.deltaTime);
        }

        wasSeeing = canSee;
    }

    /// <summary>
    /// One cell at a time, re-deciding between patrol and chase after every step,
    /// so the NPC can change its mind the moment the player ducks out of sight.
    /// </summary>
    private IEnumerator RunBehaviour()
    {
        while (true)
        {
            if (IsChasing && player != null)
            {
                yield return StepToward(
                    player.transform.position,
                    moveDuration / Mathf.Max(0.01f, chaseSpeedMultiplier));
                continue;
            }

            if (patrolPoints == null || patrolPoints.Length < 2)
            {
                yield return null;
                continue;
            }

            Vector2 target = patrolPoints[currentIndex].position;
            if (((Vector2)transform.position - target).sqrMagnitude <= 0.01f)
            {
                if (!loop && currentIndex == patrolPoints.Length - 1)
                {
                    behaviourRoutine = null;
                    yield break;
                }

                currentIndex = (currentIndex + 1) % patrolPoints.Length;
                yield return new WaitForSeconds(Mathf.Max(0f, waitAtPoint));
                continue;
            }

            yield return StepToward(target, moveDuration);
        }
    }

    private IEnumerator StepToward(Vector2 worldTarget, float duration)
    {
        if (!TryBuildPath(transform.position, worldTarget, out List<Vector2> path) || path.Count == 0)
        {
            // Unreachable this frame - a closed route, or the player standing on us.
            // Wait a beat rather than spinning the coroutine every frame.
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        Vector2 next = path[0];
        Vector2 delta = next - (Vector2)transform.position;
        if (delta.sqrMagnitude > 0.001f)
        {
            ApplyFacingRotation(Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? new Vector2(Mathf.Sign(delta.x), 0f)
                : new Vector2(0f, Mathf.Sign(delta.y)));
        }

        yield return MoveToCell(next, duration);
        SnapToGrid();
    }

    private void Catch()
    {
        suspicion = 0f;
        wasSeeing = false;

        if (player != null)
        {
            player.ResetToStart();
        }

        // Give up the chase and pick the route back up from wherever we ended, otherwise
        // the NPC camps the start point and catches the player again on sight.
        currentIndex = NearestPatrolPointIndex();
        onPlayerCaught?.Invoke();
    }

    private int NearestPatrolPointIndex()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return 0;
        }

        int best = 0;
        float bestDistance = float.MaxValue;
        for (int index = 0; index < patrolPoints.Length; index++)
        {
            if (patrolPoints[index] == null)
            {
                continue;
            }

            float distance =
                ((Vector2)patrolPoints[index].position - (Vector2)transform.position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = index;
            }
        }

        return best;
    }

    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
    }

    private IEnumerator MoveToCell(Vector2 target, float duration)
    {
        Vector3 start = transform.position;
        Vector3 end = new Vector3(target.x, target.y, start.z);
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, elapsed / safeDuration);
            yield return null;
        }

        transform.position = end;
    }

    private void SnapToGrid()
    {
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            transform.position.z);
    }

    private void ApplyFacingRotation(Vector2 facing)
    {
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;

        Transform visual = transform.Find("Visual");
        if (visual != null)
        {
            visual.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Transform cone = transform.Find("VisionCone");
        if (cone != null)
        {
            cone.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private bool TryBuildPath(Vector2 startWorld, Vector2 targetWorld, out List<Vector2> path)
    {
        const float npcCellSize = 1f;
        Vector2 collisionSize = new Vector2(0.55f, 0.55f);
        return MazeGridPathfinder2D.TryBuildPath(
            startWorld,
            targetWorld,
            npcCellSize,
            collisionSize,
            MazeLayers.Blocking,
            out path);
    }
}
