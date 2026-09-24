using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Deterministic orthographic follow camera for the Maze prototype.
/// This is intentionally independent from gameplay state so the player/view relationship is easy to verify.
/// </summary>
public sealed class MazeCameraController2D : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float followSmoothTime = 0.10f;
    [SerializeField] private float orthographicSize = 6f;
    [SerializeField] private float minOrthographicSize = 3.5f;
    [SerializeField] private float maxOrthographicSize = 9f;
    [SerializeField] private float zoomSpeed = 0.02f;    [SerializeField] private bool clampToMazeBounds = true;
    [SerializeField] private Vector2 mazeBoundsMin = new Vector2(-7.5f, -5.5f);
    [SerializeField] private Vector2 mazeBoundsMax = new Vector2(7.5f, 5.5f);



    private Vector3 followVelocity;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        ApplyLens();
    }

    private void LateUpdate()
    {
        if (targetCamera == null || followTarget == null)
        {
            return;
        }

        Vector3 desired = followTarget.position + targetOffset;
        desired.z = targetOffset.z;
        desired = ClampToMazeBounds(desired);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref followVelocity,
            Mathf.Max(0.001f, followSmoothTime));

        if (Mouse.current != null)
        {
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            if (scroll.sqrMagnitude > 0f)
            {
                orthographicSize = Mathf.Clamp(
                    orthographicSize - scroll.y * zoomSpeed,
                    minOrthographicSize,
                    maxOrthographicSize);
                ApplyLens();
            }
        }
    }

    private Vector3 ClampToMazeBounds(Vector3 desired)
    {
        if (!clampToMazeBounds || targetCamera == null)
        {
            return desired;
        }

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * Mathf.Max(0.01f, targetCamera.aspect);
        float minX = mazeBoundsMin.x + halfWidth;
        float maxX = mazeBoundsMax.x - halfWidth;
        float minY = mazeBoundsMin.y + halfHeight;
        float maxY = mazeBoundsMax.y - halfHeight;

        desired.x = minX > maxX ? (mazeBoundsMin.x + mazeBoundsMax.x) * 0.5f : Mathf.Clamp(desired.x, minX, maxX);
        desired.y = minY > maxY ? (mazeBoundsMin.y + mazeBoundsMax.y) * 0.5f : Mathf.Clamp(desired.y, minY, maxY);
        return desired;
    }


    public void SetTarget(Transform target)
    {
        followTarget = target;
    }

    public void SetOrthographicSize(float size)
    {
        orthographicSize = Mathf.Clamp(size, minOrthographicSize, maxOrthographicSize);
        ApplyLens();
    }

    private void ApplyLens()
    {
        if (targetCamera == null)
        {
            return;
        }

        targetCamera.orthographic = true;
        targetCamera.orthographicSize = orthographicSize;
    }
}
