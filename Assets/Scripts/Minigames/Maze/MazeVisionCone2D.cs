using UnityEngine;

/// <summary>
/// World-space cone renderer and line-of-sight query for the Maze prototype.
/// The source transform owns position and facing; the cone child only owns presentation.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public sealed class MazeVisionCone2D : MonoBehaviour
{
    [SerializeField] private Transform source;
    [SerializeField] private float viewDistance = 5f;
    [SerializeField, Range(1f, 180f)] private float viewAngle = 90f;
    [SerializeField, Min(8)] private int rayCount = 48;
    [SerializeField] private LayerMask occlusionMask;
    [SerializeField] private Color coneColor = new Color(1f, 0.85f, 0.35f, 0.22f);
    [SerializeField] private bool showDebugRays;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh coneMesh;

    public float ViewDistance => viewDistance;
    public float ViewAngle => viewAngle;

    private void Awake()
    {
        // See MazePlayerController2D: the serialized mask is baked against layers
        // this project does not have. MazeLayers is the only source.
        occlusionMask = MazeLayers.Blocking;

        if (source == null)
        {
            source = transform.parent != null ? transform.parent : transform;
        }

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        coneMesh = new Mesh { name = "MazeVisionCone" };
        coneMesh.MarkDynamic();
        meshFilter.sharedMesh = coneMesh;
        meshRenderer.sortingOrder = 20;
        EnsureMaterial();
    }

    private void LateUpdate()
    {
        if (source == null)
        {
            return;
        }

        // Follow the source's position but never its rotation: the maze keeps NPC and
        // player roots axis-aligned so their sprites cannot flip, and copying that
        // identity rotation every LateUpdate used to snap the cone back to world up,
        // undoing the facing the owner had just set on this transform.
        transform.position = source.position;
        RebuildMesh();
    }

    public bool CanSee(Vector2 targetPosition)
    {
        if (source == null)
        {
            return false;
        }

        Vector2 toTarget = targetPosition - (Vector2)source.position;
        float distance = toTarget.magnitude;
        if (distance > viewDistance || distance <= 0.001f)
        {
            return false;
        }

        float angle = Vector2.Angle(transform.up, toTarget);
        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.Raycast(source.position, toTarget.normalized, distance, occlusionMask);
        return hit.collider == null;
    }

    private void RebuildMesh()
    {
        int samples = Mathf.Max(8, rayCount);
        Vector3[] vertices = new Vector3[samples + 2];
        int[] triangles = new int[samples * 3];
        vertices[0] = Vector3.zero;

        float halfAngle = viewAngle * 0.5f;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector2 localDirection = DirectionFromAngle(angle);
            Vector2 worldDirection = transform.TransformDirection(localDirection);
            float distance = viewDistance;
            RaycastHit2D hit = Physics2D.Raycast(source.position, worldDirection, viewDistance, occlusionMask);
            if (hit.collider != null)
            {
                distance = hit.distance;
            }

            vertices[i + 1] = localDirection * distance;
        }

        for (int i = 0; i < samples; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }

        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateBounds();

        if (showDebugRays)
        {
            DrawDebugRays(samples, halfAngle);
        }
    }

    private void DrawDebugRays(int samples, float halfAngle)
    {
        for (int i = 0; i <= samples; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)samples);
            Vector2 direction = transform.TransformDirection(DirectionFromAngle(angle));
            RaycastHit2D hit = Physics2D.Raycast(source.position, direction, viewDistance, occlusionMask);
            Vector2 end = hit.collider == null ? (Vector2)source.position + direction * viewDistance : hit.point;
            Debug.DrawLine(source.position, end, Color.yellow);
        }
    }

    private void EnsureMaterial()
    {
        if (meshRenderer.sharedMaterial != null)
        {
            meshRenderer.sharedMaterial.color = coneColor;
            return;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            return;
        }

        Material material = new Material(shader) { name = "MazeVisionCone_Runtime" };
        material.color = coneColor;
        meshRenderer.sharedMaterial = material;
    }

    private static Vector2 DirectionFromAngle(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }

    private void OnDrawGizmosSelected()
    {
        Transform gizmoSource = source != null ? source : transform.parent != null ? transform.parent : transform;
        Gizmos.color = Color.yellow;
        Vector3 left = gizmoSource.position + (Vector3)(Quaternion.Euler(0f, 0f, -viewAngle * 0.5f) * gizmoSource.up) * viewDistance;
        Vector3 right = gizmoSource.position + (Vector3)(Quaternion.Euler(0f, 0f, viewAngle * 0.5f) * gizmoSource.up) * viewDistance;
        Gizmos.DrawLine(gizmoSource.position, left);
        Gizmos.DrawLine(gizmoSource.position, right);
    }
}
