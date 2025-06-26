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
            Debug.Log(touch_wp.x + ", " + touch_wp.y);

            for (int i = 0; i < numObjects; i++) { 
                
                //aabb bounding test
                    if (!(touch_wp.x < interactables[i].gameObject.transform.position.x - interactables[i].GetComponent<Collider2D>().bounds.size.x/2  ||
                           touch_wp.x > interactables[i].gameObject.transform.position.x + interactables[i].GetComponent<Collider2D>().bounds.size.x/2 ||
                          touch_wp.y <  interactables[i].gameObject.transform.position.y - interactables[i].GetComponent<Collider2D>().bounds.size.y / 2  ||
                           touch_wp.y > interactables[i].gameObject.transform.position.y + interactables[i].GetComponent<Collider2D>().bounds.size.y / 2) 
                           && !interactables[i].fading && interactables[i].gameObject.activeInHierarchy){

                    //Runs the dialogue if it has any
                    if (interactables[i].startNode != "")
                        {
                            runDialogue(interactables[i]);

                        }
                    //Otherwise begins the fade away
                    else
                        {
                            Debug.Log(i + " CLICKED!!!!!");
                            interactables[i].fading = true;
                        }
                        
                    }
            }

        }
    }

    //Runs the dialogue once per object and labels it finished so it won't run it again
    private void runDialogue(Interactable inter)
    {
        if (!inter.finished)
        {
            dialogueRunner.StartDialogue(inter.startNode);
            inter.finished = true;
            Debug.Log("AHHH");
        }
    }

    //Fades the objects and attemps to end the minigame when an object fades
    private void Update()
    {
        for(int i = 0; i < numObjects; i++)
        {
            if (interactables[i].fading && !interactables[i].finished)
                interactables[i].GetComponent<SpriteRenderer>().color = new Color(interactables[i].GetComponent<SpriteRenderer>().color.r,
                interactables[i].GetComponent<SpriteRenderer>().color.g, interactables[i].GetComponent<SpriteRenderer>().color.g, interactables[i].GetComponent<SpriteRenderer>().color.a - 0.001f);
                if (interactables[i].GetComponent<SpriteRenderer>().color.a <= 0 && !interactables[i].finished)
            {
                numActiveObjects--;
                interactables[i].finished = true;
                endInteractableMinigame();
            }
        }
        

    }

    //Ends the minigame if there are no more active objects left
    public void endInteractableMinigame()
    {
        Debug.Log("end method");
        if (numActiveObjects <= 0)
        {
            StopMinigame();
            FlowerShopManager.Instance.NextMinigame();
        }
    }
}
