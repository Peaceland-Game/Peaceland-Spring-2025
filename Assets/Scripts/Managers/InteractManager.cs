using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using Yarn.Unity;
using static OrderObject;

public class InteractManager : MinigameBehavior
{
    [SerializeField] GameObject InteractablePrefab;

    [SerializeField] DialogueRunner dialogueRunner;

    public Interactable[] interactables;

    public int numObjects;

    public int numActiveObjects;



    public override void StartMinigame()
    {
        numObjects = FlowerShopManager.GetCurrentOrder().flowers.Count;
        numActiveObjects = numObjects;

        for (int i = 0; i < numObjects; i++)
        {
           interactables[i] = Instantiate(interactables[i]);
           interactables[i].transform.parent = transform;

        }

        gameObject.SetActive(true);

    }
    public override void StopMinigame()
    {
        for (int i = 0; i < interactables.Length; i++)
        {
            Destroy(interactables[i]);
        }

        gameObject.SetActive(false);
    }

    public void DoClick()
    {
        if (this.gameObject.activeInHierarchy) {
        Vector3 touch_wp = InputHelper.GetPointerWorldPosition();

            for (int i = 0; i < numObjects; i++) { 
                
                    if (!(touch_wp.x < interactables[i].gameObject.transform.position.x - interactables[i].GetComponent<Collider2D>().bounds.size.x/2  ||
                           touch_wp.x > interactables[i].gameObject.transform.position.x + interactables[i].GetComponent<Collider2D>().bounds.size.x/2 ||
                          touch_wp.y <  interactables[i].gameObject.transform.position.y - interactables[i].GetComponent<SpriteRenderer>().bounds.size.y / 2  ||
                           touch_wp.y > interactables[i].gameObject.transform.position.y + interactables[i].GetComponent<SpriteRenderer>().bounds.size.y / 2) 
                           && !interactables[i].fading && interactables[i].gameObject.activeInHierarchy){
                    if (interactables[i].hasDialogue)
                        {
                            runDialogue(interactables[i]);

                        }
                    else
                        {
                            Debug.Log(i + " CLICKED!!!!!");
                            interactables[i].fading = true;
                        }
                        
                    }
            }

        }
    }

    private void runDialogue(Interactable inter)
    {
        if (!inter.finished)
        {
            dialogueRunner.StartDialogue(inter.startNode);
            inter.finished = true;
            Debug.Log("AHHH");
        }
    }

    private void Update()
    {
        for(int i = 0; i < numObjects; i++)
        {
            if (interactables[i].fading && !interactables[i].finished)
                interactables[i].GetComponent<SpriteRenderer>().color = new Color(interactables[i].GetComponent<SpriteRenderer>().color.r,
                interactables[i].GetComponent<SpriteRenderer>().color.g, interactables[i].GetComponent<SpriteRenderer>().color.g, interactables[i].GetComponent<SpriteRenderer>().color.a - 0.005f);
                if (interactables[i].GetComponent<SpriteRenderer>().color.a <= 0 && !interactables[i].finished)
            {
                numActiveObjects--;
                interactables[i].finished = true;
                endInteractableMinigame();
            }
        }
        

    }

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
