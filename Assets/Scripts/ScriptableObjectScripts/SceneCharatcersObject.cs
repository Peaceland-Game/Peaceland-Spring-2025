using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneCharatcers", menuName = "Scriptable Objects/SceneCharatcers")]
public class SceneCharatcersObject : ScriptableObject
{
    /// <summary>
    /// Sprintes for main character, appears on the left of the sceen
    /// </summary>
    [SerializeField]
    protected Sprite[] mainCharSprites;
    /// <summary>
    /// Get property for the main character's sprite array
    /// </summary>
    public Sprite[] MainCharSprites { get { return mainCharSprites; } }

    /// <summary>
    /// Sprites for secondary character, appears on the right of the screen
    /// </summary>
    [SerializeField]
    protected Sprite[] secondCharSprites;
    /// <summary>
    /// Get property for the secondary character's sprite array
    /// </summary>
    public Sprite[] SecondCharSprites { get { return secondCharSprites; } }

    // These were in the original object. Not sure if they do anything.
    [SerializeField]
    protected string dialogueStartNode;
    [SerializeField]
    protected string dialogueEndNode;
    [SerializeField]
    protected string nameForScene;
}
