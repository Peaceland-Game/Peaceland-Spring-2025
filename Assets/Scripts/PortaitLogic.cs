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
        dialogueRunner.AddCommandHandler<int, int>("changeSprite", ChangeSprite);
        dialogueRunner.AddCommandHandler("oneChar", OneChar);
        dialogueRunner.AddCommandHandler("twoChar", TwoChar);
        dialogueRunner.AddCommandHandler("zeroChar", ZeroChar);
        dialogueRunner.AddCommandHandler("nextOrder", NextOrder);
    }

    /// <summary>
    /// Makes the lighten and darken able to happen with only one method call for any typical speaking
    /// </summary>
    /// <param name="i">0 for player, 1 for left/main char, 2 for right char</param>
    private void Speaker(int i = 0)
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
    private void Darken(int i = 1)
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
    private void Lighten(int i = 1)
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
        characterPortrait.transform.position = new Vector3(-3f, -0.5f, 0f);

    }

    /// <summary>
    /// Makes two npcs show on screen
    /// </summary>
    private void TwoChar()
    {
        secondCharacterPortrait.SetActive(true);
        characterPortrait.SetActive(true);
        characterPortrait.transform.position = new Vector3(-4f, -0.5f, 0f);
        secondCharacterPortrait.transform.position = new Vector3(5f, -0.5f, 0f);
    }

    /// <summary>
    /// Makes no character show on screen
    /// </summary>
    private void ZeroChar()
    {
        characterPortrait.SetActive(false);
        secondCharacterPortrait.SetActive(false);
    }

    /// <summary>
    /// Changes a character's facial expression
    /// </summary>
    /// <param name="character">Which character should change</param>
    /// <param name="portait">What portrait to change to</param>
    private void ChangeSprite(int character, int portait)
    {
        if (character == 1)
        {
            if (FlowerShopManager.GetMainSprites().Length > portait)
            {
                characterPortrait.GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetMainSprites()[portait];
            }
        }
        else
        {
            if (FlowerShopManager.GetSecondSprites().Length > portait)
            {
                secondCharacterPortrait.GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetSecondSprites()[portait];
            }
        }
    }

    /// <summary>
    /// Skip to next order without modifying other things like character sprite or opening the door
    /// </summary>
    private void NextOrder()
    {
        FlowerShopManager.NextOrder();
    }
}