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
        /// <summary>
        /// name of the flower
        /// </summary>
        public string name;

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

        public enum TypeOfFlower
        {
            ROSE,
            TULIP,
            SUNFLOWER,
            LILY,
            GLADIOLUS
        }
        public TypeOfFlower flowerType;

        /// <summary>
        /// The texture/sprite of the flower
        /// </summary>
        [HideInInspector] public Texture2D texture;

        public void Start()
        {
            isFinished = false;
            texture = (Texture2D)Resources.Load("Assets/Art/Flowers/Blue/blue_flower_no_thorns.png");
            Debug.Log(texture);
        }
    }
}
