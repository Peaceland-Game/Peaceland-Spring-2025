using UnityEngine;

/// <summary>
/// SUPERSEDED by MazeLayers. This resolved a Layer by name at runtime back when the
/// maze expected Layers the project does not have; the maze now uses the project's
/// own Wall Layer and reads it from one place. Nothing references this any more, and
/// Peaceland > Maze > Rebake Element Layers strips it off the prefabs it finds it on.
/// Kept so an older scene that still carries one keeps loading.
/// </summary>
[DefaultExecutionOrder(-12000)]
public sealed class MazeLayerBinding : MonoBehaviour
{
    [SerializeField] private string layerName;
    [SerializeField] private bool includeChildren = true;

    private void Awake()
    {
        Apply();
    }

    public void Configure(string value)
    {
        layerName = value;
        Apply();
    }

    private void Apply()
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogError("Required maze layer is missing: " + layerName, this);
            return;
        }

        gameObject.layer = layer;
        if (!includeChildren)
        {
            return;
        }

        foreach (Transform item in GetComponentsInChildren<Transform>(true))
        {
            item.gameObject.layer = layer;
        }
    }
}
