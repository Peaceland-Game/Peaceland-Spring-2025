using UnityEngine;
using Yarn.Unity;

public class PortaitLogic : MonoBehaviour
    //Logic for lightening and darkening portraits by giving commands to yarnspinner
{
    [SerializeField]
    private DialogueRunner dialogueRunner;

    [SerializeField]
    private GameObject characterPortrait;

    [SerializeField]
    private GameObject secondCharacterPortrait;

    public void Awake()
    {
        //Adds commands that can be called in yarn using <<lighten>> and <<darken>> respectively
        dialogueRunner.AddCommandHandler<int>("lighten", Lighten);
        dialogueRunner.AddCommandHandler<int>("darken", Darken);
        dialogueRunner.AddCommandHandler("oneChar", OneChar);
        dialogueRunner.AddCommandHandler("twoChar", TwoChar);
    }

    private void Darken(int i)
    {
        if (i == 1)
        {
            characterPortrait.GetComponent<SpriteRenderer>().color = new Color(.75f, .75f, .75f, 1f);
        }
        else
        {
            secondCharacterPortrait.GetComponent<SpriteRenderer>().color = new Color(.75f, .75f, .75f, 1f);
        }
    }

    private void Lighten(int i)
    {
        if (i == 1)
        {
            characterPortrait.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            secondCharacterPortrait.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void OneChar()
    {
        characterPortrait.transform.position = new Vector3(-3, -1, 0);
        secondCharacterPortrait.SetActive(false);
    }

    private void TwoChar()
    {
        characterPortrait.transform.position = new Vector3(-5, -1, 0);
        secondCharacterPortrait.transform.position = new Vector3(5, -1, 0);
        secondCharacterPortrait.SetActive(true);
    }
}
