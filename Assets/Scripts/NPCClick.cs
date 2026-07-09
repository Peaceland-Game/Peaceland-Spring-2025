using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class NPCClick : MonoBehaviour
{
    //Variables for dialogue and colliders
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private BoxCollider2D boxCollider;

    Vector3 originalPosition;
    Vector3 originalScale;
    private bool npcMoved = false;

    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;

        dialogueRunner.onDialogueComplete.AddListener(RestoreNPC); //When dialogue ends, the original position and scales are restored
    }

    /// <summary>
    /// The code grabs the name of the game object of the corresponding box collider and puts it in with the starting node.
    /// This allows the script to work with any NPC, as well as tree dialogue instead of making separate scripts for each NPC and the tree.
    /// The code will only work, however, if the nodes include the game object's name and "Start" afterwards.
    /// 
    /// When it comes to starting the memories, the individual scenes don't work. The R&J demo for example can open, but will not get past the newspaper scene due to missing references.
    /// This means that starting memories can be done, but they will need to be tweaked in order to properly run them.
    /// 
    /// Some NPCs I've decided to not be able to not have the option to move onto a memory, as the game only has 2 memories at the moment.
    /// 
    /// This is also where the position and scales are changed, which, while the colliders are also modified, do not have any effect. So the massive tree collider is not anything to worry about.
    /// </summary>
    void Update()
    {
        if (!dialogueRunner.IsDialogueRunning)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                if (boxCollider.OverlapPoint(mouseWorld))
                {
                    GameObject npc = gameObject;
                    originalPosition = npc.transform.position;
                    originalScale = npc.transform.localScale;
                    Debug.Log($"Clicked {npc.name}");

                    //The dialogue only works if the game object name is in the starting dialogue title followed by "Start", which allows this code to work with all NPCs
                    dialogueRunner.StartDialogue(npc.name + "Start");
                    npc.transform.position = new Vector3(-4f, -3f, 0f);
                    npc.transform.localScale = new Vector3(transform.localScale.x * 3f, transform.localScale.y * 3f, transform.localScale.z);
                    npcMoved = true;
                }
            }
        }
        if (dialogueRunner.CurrentNodeName == "MarciaStoryStart" || dialogueRunner.CurrentNodeName == "JohnnyStoryStart")
        {
            SceneManager.LoadScene("DemoStart");
        }
    }

    /// <summary>
    /// This takes the original npc position and scales and restores them
    /// </summary>
    void RestoreNPC()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        npcMoved = false;
    }
}
