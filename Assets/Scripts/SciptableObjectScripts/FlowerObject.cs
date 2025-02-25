using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FlowerObject", menuName = "Scriptable Objects/FlowerObject")]
public class FlowerObject : ScriptableObject
{
    /// <summary>
    /// The name that was given for the flower order
    /// </summary>
    public string nameForOrder;

    /// <summary>
    /// List of flowers as Texture2D (subject to change)
    /// </summary>
    public List<GameObject> flowers;
}
