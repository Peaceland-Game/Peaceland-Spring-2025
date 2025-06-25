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
        //Adds commands that can be called in yarn using the name in quotes
        dialogueRunner.AddCommandHandler<int>("speaker", Speaker);
        dialogueRunner.AddCommandHandler<int>("lighten", Lighten);
        dialogueRunner.AddCommandHandler<int>("darken", Darken);
        dialogueRunner.AddCommandHandler("oneChar", OneChar);
        dialogueRunner.AddCommandHandler("twoChar", TwoChar);
        dialogueRunner.AddCommandHandler("zeroChar", ZeroChar);
    }

    /// <summary>
    /// Makes the lighten and darken able to happen with only one method call for any typical speaking
    /// </summary>
    /// <param name="i">0 for player, 1 for left/main char, 2 for right char</param>
    private void Speaker(int i=0)
    {
        switch (i)
        {
            case 0:
                Darken(1);
                Darken(2);
                break;
            case 1:
                Lighten(1);
                Darken(2);
                break;
            case 2:
                Darken(1);
                Lighten(2);
                break;
        }
    }

    /// <summary>
    /// Darkens a particular characer, main char by default
    /// </summary>
    /// <param name="i">1 for left/main char, 2 for right char</param>
    private void Darken(int i=1)
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

    /// <summary>
    /// Lightens a particular character, main char by default
    /// </summary>
    /// <param name="i">1 for left/main char, 2 for right char</param>
    private void Lighten(int i=1)
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

    /// <summary>
    /// Makes only the main npc show on screen
    /// </summary>
    private void OneChar()
    {
        characterPortrait.SetActive(true);
        secondCharacterPortrait.SetActive(false);
        characterPortrait.transform.position = new Vector3(-3, -1, 0);
        
    }

    /// <summary>
    /// Makes two npcs show on screen
    /// </summary>
    private void TwoChar()
    {
        secondCharacterPortrait.SetActive(true);
        characterPortrait.SetActive(true);
        characterPortrait.transform.position = new Vector3(-5, -1, 0);
        secondCharacterPortrait.transform.position = new Vector3(5, -1, 0);
    }

    private void ZeroChar()
    {
        characterPortrait.SetActive(false);
        secondCharacterPortrait.SetActive(false);
    }
}
