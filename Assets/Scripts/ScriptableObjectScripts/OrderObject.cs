using UnityEngine;
using System.Collections.Generic;
using System;

public enum FlowerType
{
    ROSE,
    DAISY,
    PINKROSE,
    THISTLE,
    CHAMOMILE,
    GLADIOLUS,
    HAWTHORN,
    CARNATION,
    ORCHID
}

[CreateAssetMenu(fileName = "OrderObject", menuName = "Scriptable Objects/OrderObject")]
public class OrderObject : ScriptableObject
{
    /// <summary>
    /// The name that was given for the flower order
    /// </summary>
    public string nameForOrder;

    /// <summary>
    /// The sprites associated with the main character for the flower order
    /// </summary>
    public Sprite[] mainCharSprites;

    /// <summary>
    /// The sprites associated with the seoncdary character for the flower order
    /// </summary>
    public Sprite[] secondCharSprites;

    /// <summary>
    /// List of flowers as Texture2D (subject to change)
    /// </summary>
    public List<Flower> flowers;

    public string dialogueStartNode;
    public string dialogueEndNode;

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