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
        public string name;
        public Texture2D texture;


    }

}
