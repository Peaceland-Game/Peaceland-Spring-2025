using UnityEngine;

/// <summary>
/// The one place the maze decides what counts as solid.
/// The maze deliberately adds no layers of its own: the project already has Wall,
/// and a wall that stops a step also stops a sight line.
/// </summary>
public static class MazeLayers
{
    public const string WallLayerName = "Wall";

    /// <summary>
    /// Blocks movement, pathfinding and line of sight alike.
    /// Callers cache this; it is a lookup, not a field.
    /// </summary>
    public static LayerMask Blocking => LayerMask.GetMask(WallLayerName);
}
