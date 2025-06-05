using UnityEngine;
using Yarn.Unity;

public class PortaitLogic : MonoBehaviour
    //Logic for lightening and darkening portraits by giving commands to yarnspinner
{
    [SerializeField]
    private DialogueRunner dialogueRunner;

    [SerializeField]
    private SpriteRenderer characterSprite;

    public void Awake()
    {
        //Adds commands that can be called in yarn using <<lighten>> and <<darken>> respectively
        dialogueRunner.AddCommandHandler("lighten", Lighten);
        dialogueRunner.AddCommandHandler("darken", Darken);
    }

    private void Darken()
    {
        characterSprite.color = new Color(.75f, .75f, .75f, 1f);
    }

    private void Lighten()
    {
        characterSprite.color = Color.white;
    }
}
