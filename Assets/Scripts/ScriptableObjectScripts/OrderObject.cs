using UnityEngine;
using System.Collections.Generic;
using System;

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

    [Serializable]
    public struct Flower
    {
        //Name of the flower
        public string name;
        //The texture/sprite of the flower
        public Texture2D texture;

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
        /// Is the flower finished being created?
        /// </summary>
        public bool isFinished;

        public void Start()
        {
            isFinished = false;
        }
    }
}
