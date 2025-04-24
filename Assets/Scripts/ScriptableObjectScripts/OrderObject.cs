using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Allows for code to handle setting up the appropirate flower, according to the given flower type
/// </summary>
public enum FlowerType
{
    ROSE,
    DAISY,
}

[CreateAssetMenu(fileName = "OrderObject", menuName = "Scriptable Objects/OrderObject")]
public class OrderObject : ScriptableObject
{
    /// <summary>
    /// The name that was given for the flower order
    /// </summary>
    public string nameForOrder;

    /// <summary>
    /// List of flowers as Texture2D (subject to change)
    /// </summary>
    public List<Flower> flowers;

    public string dialogueStartNode;
    public string dialogueEndNode;

    /// <summary>
    /// A "flower", holding values for if it needs dethorning, trimming, arrnaging, and what FlowerType it is
    /// </summary>
    [Serializable]
    public struct Flower
    {
        /// <summary>
        /// Does the current flower need dethorning?
        /// </summary>
        public bool needsDethorning;

        /// <summary>
        /// Does the current flower need trimming?
        /// </summary>
        public bool needsTrimming;

        /// <summary>
        /// Does the current flower need arranging?
        /// </summary>
        public bool needsArranging;

        /// <summary>
        /// Helps with assigning the appropriate flower sprite in gameplay
        /// </summary>
        public FlowerType flowerType;
    }
}
