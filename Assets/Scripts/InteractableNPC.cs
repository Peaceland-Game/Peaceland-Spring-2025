using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// A basic script for an interactable NPC. 
/// The NPC has two states: Interactable and Not Interactable
/// The NPC will become interactable when the Player (a trigger) enters a given radius
/// When the NPC is Interactable, its sprite will change as well
/// </summary>
public class InteractableNPC : MonoBehaviour
{
    [SerializeField] float interactionRadius;
    [SerializeField] bool isInteractable = false;

    [SerializeField] Sprite activeSprite;
    [SerializeField] Sprite notActiveSprite;

    [SerializeField] GameObject player;


    float distBetween = 0;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        distBetween = this.transform.position.sqrMagnitude - player.transform.position.sqrMagnitude;
        if(distBetween < interactionRadius)
        {
            // Change to interactable
            isInteractable = true;

            // Change sprite
            spriteRenderer.sprite = activeSprite;
        }
        else
        {
            // Change to not interactable
            isInteractable = false;

            // Change sprite back to default
            spriteRenderer.sprite = notActiveSprite;
        }
    }
}
