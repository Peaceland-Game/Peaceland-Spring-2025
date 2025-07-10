using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using Yarn.Unity;
using static OrderObject;

public class InteractManager : MinigameBehavior
{
    /// <summary>
    /// Runs the dialogue when an object with dialogue is clicked on
    /// </summary>
    [SerializeField] DialogueRunner dialogueRunner;

    /// <summary>
    /// List of the interactable objects
    /// </summary>
    public Interactable[] interactables;

    /// <summary>
    /// Number of interactable objects in this minigame
    /// </summary>
    public int numObjects;

    /// <summary>
    /// Number of active interactable objects
    /// </summary>
    public int numActiveObjects;

    /// <summary>
    /// Marks if something has been clicked in this loop to prevent multi clicks
    /// </summary>
    private bool somethingClicked = false;


    /// <summary>
    /// Instantiates the interactable objects and begins the minigame
    /// </summary>
    public override void StartMinigame()
    {
        numObjects = interactables.Length;
        numActiveObjects = numObjects;

        for (int i = 0; i < numObjects; i++)
        {
           interactables[i] = Instantiate(interactables[i]);
           interactables[i].transform.parent = transform;

        }

        gameObject.SetActive(true);

    }
    /// <summary>
    /// Destorys the interactable objects and ends the minigame
    /// </summary>
    public override void StopMinigame()
    {
        for (int i = 0; i < interactables.Length; i++)
        {
            Destroy(interactables[i]);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// This is the method that detects when the object is clicked on. I could not figure out the
    /// regular on click methods using the input systems package, so I used the onTap method in
    /// GrabAndSwipe to run this method.
    /// </summary>
    public void DoClick()
    {

        if (this.gameObject.activeInHierarchy) {
        Vector3 touch_wp = InputHelper.GetPointerWorldPosition();

            for (int i = 0; i < numObjects; i++) { 
                
                //aabb bounding test
                    if (!(touch_wp.x < interactables[i].GetComponent<Collider2D>().attachedRigidbody.position.x - interactables[i].GetComponent<Collider2D>().bounds.size.x/2  ||
                           touch_wp.x > interactables[i].GetComponent<Collider2D>().attachedRigidbody.position.x + interactables[i].GetComponent<Collider2D>().bounds.size.x/2 ||
                          touch_wp.y < interactables[i].GetComponent<Collider2D>().attachedRigidbody.position.y - interactables[i].GetComponent<Collider2D>().bounds.size.y / 2  ||
                           touch_wp.y > interactables[i].GetComponent<Collider2D>().attachedRigidbody.position.y + interactables[i].GetComponent<Collider2D>().bounds.size.y / 2) 
                           && !interactables[i].fading && interactables[i].gameObject.activeInHierarchy && !somethingClicked){

                    somethingClicked = true;

                    interactables[i].GetComponent<Animator>().enabled = false;

                    //Runs the dialogue if it has any
                    if (interactables[i].startNode != "")
                        {
                            runDialogue(interactables[i]);

                        }
                    //Otherwise begins the fade away
                    if(interactables[i].startedDialogue || interactables[i].startNode == "")
                        interactables[i].fading = true;
                    }
            }

        }
        somethingClicked = false;
    }

    //Runs the dialogue once per object and labels it finished so it won't run it again
    private void runDialogue(Interactable inter)
    {
        if (!inter.startedDialogue && !dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue(inter.startNode);
            inter.startedDialogue = true;
        }
    }

    //Fades the objects and attemps to end the minigame when an object fades
    private void Update()
    {
        for(int i = 0; i < numObjects; i++)
        {
            if (interactables[i].fading && !interactables[i].finishedFading && !dialogueRunner.IsDialogueRunning)
                interactables[i].GetComponent<SpriteRenderer>().color = new Color(interactables[i].GetComponent<SpriteRenderer>().color.r,
                interactables[i].GetComponent<SpriteRenderer>().color.g, interactables[i].GetComponent<SpriteRenderer>().color.g, interactables[i].GetComponent<SpriteRenderer>().color.a - (0.9f * Time.deltaTime));
                if (interactables[i].GetComponent<SpriteRenderer>().color.a <= 0 && !interactables[i].finishedFading && !dialogueRunner.IsDialogueRunning)
            {
                numActiveObjects--;
                interactables[i].finishedFading = true;
                endInteractableMinigame();

            }
        }
        

    }

    //Ends the minigame if there are no more active objects left
    public void endInteractableMinigame()
    {
        if (numActiveObjects <= 0)
        {
            StopMinigame();
            FlowerShopManager.Instance.InteractableDialogueNextMinigame();
        }
    }
}
