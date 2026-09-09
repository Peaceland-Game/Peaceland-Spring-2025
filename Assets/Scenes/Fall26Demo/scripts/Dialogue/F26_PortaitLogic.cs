using UnityEngine;
using Yarn.Unity;

public class F26_PortaitLogic : MonoBehaviour
//Logic for lightening and darkening portraits by giving commands to yarnspinner
{
    [SerializeField]
    private DialogueRunner dialogueRunner;

    [SerializeField]
    private GameObject characterPortrait;

    [SerializeField]
    private GameObject secondCharacterPortrait;

    //    [SerializeField]
    // private GenericMemManager currentMemManager;
    private F26_GameManager GM;

    public void Awake()
    {
        //Adds commands that can be called in yarn using the name in quotes
        dialogueRunner.AddCommandHandler<int>("speaker", Speaker);
        dialogueRunner.AddCommandHandler<int>("lighten", Lighten);
        dialogueRunner.AddCommandHandler<int>("darken", Darken);
        dialogueRunner.AddCommandHandler<int, int>("changeSprite", ChangeSprite);
        dialogueRunner.AddCommandHandler<int>("changeBG", ChangeBackground);
        dialogueRunner.AddCommandHandler("oneChar", OneChar);
        dialogueRunner.AddCommandHandler("twoChar", TwoChar);
        dialogueRunner.AddCommandHandler("zeroChar", ZeroChar);
        dialogueRunner.AddCommandHandler<int>("showChar", ShowChar);
        dialogueRunner.AddCommandHandler<int>("hideChar", HideChar);
        dialogueRunner.AddCommandHandler("nextOrder", NextOrder);
        dialogueRunner.AddCommandHandler<int>("showIMG", ShowObjectImage);
        dialogueRunner.AddCommandHandler<int>("hideIMG", HideObjectImage);
    }

    public void Start()
    {
        GM = F26_GameManager.Instance;

        // Find the memory manager on start
    //    GM.CurrentMemoryManager = FindFirstObjectByType<GenericMemManager>();
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
            characterPortrait.GetComponent<SpriteRenderer>().color = new Color(.7f, .7f, .75f, 1f);
        }
        else
        {
            secondCharacterPortrait.GetComponent<SpriteRenderer>().color = new Color(.7f, .7f, .75f, 1f);
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
        secondCharacterPortrait.transform.position = new Vector3(3f, -0.5f, 0f);
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
    /// Shows a character sprite
    /// </summary>
    /// <param name="characterNum">A number for which character to show. 1 for main, 2 for second.</param>
    private void ShowChar(int characterNum)
    {
        if (characterNum == 1)
        {
            characterPortrait.SetActive(true);
        }
        else if (characterNum == 2)
        {
            secondCharacterPortrait.SetActive(true);
        }
        else
        {
            Debug.Log("Attempting to show invalid character portrait. Can only show 1 or 2.");
        }
    }

    /// <summary>
    /// Hides a character sprite when not in use.
    /// </summary>
    /// <param name="characterNum">A number for which character to hide. 1 for main, 2 for second.</param>
    private void HideChar(int characterNum)
    {
        if (characterNum == 1)
        {
            characterPortrait.SetActive(false);
        }
        else if (characterNum == 2)
        {
            secondCharacterPortrait.SetActive(false);
        }
        else 
        {
            Debug.Log("Attempting to hide invalid character portrait. Can only hide 1 or 2.");
        }
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
            if (GM.CurrentMemoryManager.GetMainSprites().Length > portait)
            {
                characterPortrait.GetComponent<SpriteRenderer>().sprite = GM.CurrentMemoryManager.GetMainSprites()[portait];
            }
        }
        else
        {
            if (GM.CurrentMemoryManager.GetSecondSprites().Length > portait)
            {
                secondCharacterPortrait.GetComponent<SpriteRenderer>().sprite = GM.CurrentMemoryManager.GetSecondSprites()[portait];
            }
        }
    }

    /// <summary>
    /// Changes the background in a scene.
    /// </summary>
    /// <param name="backgroundIndex">The index of the background to change to.</param>
    private void ChangeBackground(int backgroundIndex)
    {
        GM.CurrentMemoryManager.ChangeBackgroundSprite(backgroundIndex);
    }

    /// <summary>
    /// Skip to next order without modifying other things like character sprite or opening the door
    /// </summary>
    private void NextOrder()
    {
        Debug.Log("Next Order called from Yarn Spinner");
        GM.CurrentMemoryManager.NextOrder();
    }

    /// <summary>
    /// Show an image in the scene.
    /// </summary>
    /// <param name="_index">The index of the image to show.</param>
    private void ShowObjectImage(int _index)
    {
        GM.CurrentMemoryManager.ShowObjectImage(_index);
    }

    /// <summary>
    /// Hide an image in the scene.
    /// </summary>
    /// <param name="_index">The index of the image to hide.</param>
    private void HideObjectImage(int _index)
    {
        GM.CurrentMemoryManager.HideObjectImage(_index);
    }
}