using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Events;
using System.Transactions;
using NUnit.Framework.Constraints;
using Yarn.Unity;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Interactable : MonoBehaviour
{
    /// <summary>
    /// Used to fade the object away and remove it after its done
    /// </summary>
    public bool fading;
    /// <summary>
    /// Used to label objects as complete after they finish fading
    /// </summary>
    public bool finishedFading = false;

    /// <summary>
    /// Used to label objects that have begun their dialogue
    /// </summary>
    public bool startedDialogue = false;
    /// <summary>
    /// Potentially controls the zoom
    /// </summary>
    public bool zoom;

    /// <summary>
    /// The attached dialogue. Leave blank for no dialogue
    /// </summary>
    public string startNode;

    //The position of the object
    public float x;
    public float y;
    public float z;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = new(x, y, z);
    }
}
