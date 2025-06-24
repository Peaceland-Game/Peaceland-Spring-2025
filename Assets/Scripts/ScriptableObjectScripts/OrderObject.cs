using UnityEngine;
using System.Collections.Generic;
using System;

public enum OrderObjectType
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
    /// The sprite associated with the character for the flower order
    /// </summary>
    public Sprite spriteForOrder;

    /// <summary>
    /// List of flowers as Flower object (subject to change)
    /// </summary>
    public List<OObject> objects;

    public string dialogueStartNode;
    public string dialogueEndNode;

    //add more bools or whatever as needed in accordance to whatever object
    [Serializable]
    public struct OObject
    {
        /// <summary>
        /// Does the current object need dethorning? (flowers only)
        /// </summary>
        public bool needsDethorning;

        /// <summary>
        /// Does the current object need trimming? (flowers only)
        /// </summary>
        public bool needsTrimming;

        /// <summary>
        /// Does the current object need arranging?
        /// </summary>
        public bool needsArranging;

        /// <summary>
        /// Helps with assigning the appropriate object sprite in gameplay
        /// </summary>
        public OrderObjectType objectType;
    }
}
