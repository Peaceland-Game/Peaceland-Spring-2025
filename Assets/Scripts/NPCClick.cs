using UnityEngine;
using Yarn.Unity;

public class NPCClick : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;

    //void OnMouseDown()
    //{
    //    Debug.Log("NPC clicked!");
    //    dialogueRunner.StartDialogue("MarciaStart");
    //}
    void Update()
    {
        // 0 = Left click, 1 = Right click, 2 = Middle click
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left mouse button clicked!");
        }
    }
}
