using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RoomTransition : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] DialogueRunner dialogueRunner;

    /// <summary>
    /// The idea behind this is that this could be used to transition between the different rooms in the museum (If there is ever more than the lobby and war room)
    /// 
    /// How this could work is the game object could have the same name as the scene, that way it can be used as "SceneManager.LoadScene({gameObject.name});".
    /// This would allow the code to be reused throughout the game when it comes to the room transitions.
    /// 
    /// It also checks to make sure dialogue is not currently running before transitioning rooms, to prevent accidental transition during dialogue.
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
                    SceneManager.LoadScene(gameObject.name);
                    Debug.Log("This will transition you to the " + gameObject.name + " once the branches are merged");
                }
            }
        }
    }
}
