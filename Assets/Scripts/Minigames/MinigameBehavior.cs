
using UnityEngine;

public abstract class MinigameBehavior : MonoBehaviour
{
    /// <summary>
    /// Allows children to overwrite and decide their own start methods
    /// </summary>
    public abstract void StartMinigame();
    /// <summary>
    /// Allows children to overwrite and decide their own stop methods
    /// </summary>
    public abstract void StopMinigame();
}