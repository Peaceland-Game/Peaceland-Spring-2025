using UnityEngine;
using Yarn.Unity;

public class PortaitLogic : MonoBehaviour
    //Logic for lightening and darkening portraits by giving commands to yarnspinner
{
    [SerializeField]
    private DialogueRunner dialogueRunner;

    [SerializeField]
    private GameObject characterPortrait;

    public void Awake()
    {
        dialogueRunner.AddCommandHandler("lighten", Lighten);
        dialogueRunner.AddCommandHandler("darken", Darken);
    }

    //Darken/Lighten Testing
    private void Darken()
    {
        characterPortrait.GetComponent<SpriteRenderer>().color = new Color(194, 194, 194);
        Debug.Log("Darken!");
    }

    //Darken/Lighten Testing
    private void Lighten()
    {
        characterPortrait.GetComponent<SpriteRenderer>().color = Color.white;
        Debug.Log("Lighten!");
    }
}
