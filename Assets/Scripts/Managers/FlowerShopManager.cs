using UnityEngine;
using System.Collections.Generic;
using Yarn.Unity;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class FlowerShopManager : GenericMemManager
{
    /// <summary>
    /// List of "minigames"
    /// </summary>
    //[SerializeField]
    //private List<MinigameBehavior> minigames;
    //private int currentMinigame = -1;

    [SerializeField]
    private List<OrderObject> orders;

    public List<OrderObject> Orders { get => orders; }

    /// <summary>
    /// Index of the current flower in the current order
    /// </summary>
    public static int currentFlower = 0;

    /// <summary>
    /// Singleton Instance
    /// </summary>
    public static FlowerShopManager Instance { get; private set; }

    /// <summary>
    /// Node for random dialogue when doing well.
    /// </summary>
    [SerializeField]
    private string gameplayGoodNode;

    //[SerializeField]
    //private DialogueRunner dialogueRunner;

    /// <summary>
    /// List of sprites associated with flower types
    /// </summary>
    [SerializeField]
    private Sprite[] flowerTopSprites;

    [SerializeField]
    private Sprite[] flowerBottomSprites;

    /// <summary>
    /// The current order being worked on
    /// </summary>
//    private static int currentOrder = -1;

    /// <summary>
    /// Gives access to the current order in the minigame
    /// </summary>
    /// <returns>The current order being worked on</returns>
    public static new OrderObject GetCurrentOrder()
    {
        return Instance.orders[Instance.currentOrder];
    }

    /// <summary>
    /// Increment the current order and go to the next one
    /// </summary>
    public static new void NextOrder()
    {
       Instance.currentOrder++;
    }

    #region Problem Area: Sprite Arrays
    /* The problem:
     * Memory managers are being reworked to inherit shared behaviors from an abstract generic class.
     * Many minigames in the Florist Memory require static methods to get data from the FlowerShopManager.
     * Static methods cannot override the virtual methods from the generic class.
     * PortraitLogic.cs needs to get character sprites from a non-static method, NextOrderMinigame needs
     *      to get them from a static method.
     *      
     * The Band-Aid Solution: 
     * Evan renamed the original "GetXSprite" methods to be "GetXSpritesStatic" which call the new override 
     *     "GetXSprite" methods.
     * Now both PortraitLogic and the minigame scripts can get the sprite arrays.
     * 
     * Future consideration:
     * Certain parts of the codebase should be reworked, but are not in scope for Spring 2026.
     * NextOrderMinigame loads new sprites and orders, as well as playing the door opening animation.
     *      Considering other memories also need to load sprites and change backgrounds, there should
     *      be a way to do so in a way that works for any memory.
     * The "new" keyword allows static methods to "hide" the base class methods from the compiler, 
     *      but is not proper polymorphism. It may be best to refactor the Florist Memory to not need 
     *      static methods, or refactor other memory segments to be static for consistency.
     */

    /// <summary>
    /// Returns an array of the main character's sprites
    /// </summary>
    /// <returns>A sprite array</returns>
    public override Sprite[] GetMainSprites()
    {
        return GetCurrentOrder().MainCharSprites;
    }

    /// <summary>
    /// Returns an array of the secondary character's sprites, if they exist
    /// </summary>
    /// <returns></returns>
    public override Sprite[] GetSecondSprites()
    {
        if (GetCurrentOrder().SecondCharSprites.Length > 0)
        {
            return GetCurrentOrder().SecondCharSprites;
        }
        return null;
    }

    /// <summary>
    /// Static work-around to get the Main Character's sprite array
    /// </summary>
    public static Sprite[] GetMainSpritesStatic()
    {
        return Instance.GetMainSprites();
    }

    /// <summary>
    /// Static work-around to get the Second Character's sprite array
    /// </summary>
    public static Sprite[] GetSecondSpritesStatic()
    {
        return Instance.GetSecondSprites();
    }
    #endregion

    /// <summary>
    /// The index of the current minigame that this memory is on
    /// </summary>
    /// <returns>The index of the current minigame this memory is on</returns>
    public static new int GetCurrentMinigameIndex()
    {
        return Instance.currentMinigame;
    }

    /// <summary>
    /// The current minigame that this memory is on
    /// </summary>
    /// <returns>The current minigame this memory is on</returns>
    public static new MinigameBehavior GetCurrentMinigame()
    {
        return Instance.minigames[GetCurrentMinigameIndex()];
    }

    /// <summary>
    /// The current flower that this memory is on
    /// </summary>
    /// <param name="flowerNum">The index to search for in the flowers list in the current order (-1 uses the current flower 
    /// in the FlowerShopManager instance)</param>
    /// <returns>The current flower in the current order</returns>
    public static OrderObject.Flower GetCurrentFlower(int flowerNum = -1)
    {
        if (flowerNum < 0)
        {
            return GetCurrentOrder().flowers[currentFlower];
        }
        else
        {
            return GetCurrentOrder().flowers[flowerNum];
        }
    }

    /// <summary>
    /// Gets the correct flower sprite that matches the given flower type
    /// </summary>
    /// <param name="t">The given flower type to search for</param>
    /// <returns>The flower sprite that matches the given one</returns>
    public static Sprite GetFlowerTopSprite(FlowerType t)
    {
        return Instance.flowerTopSprites[(int)t];
    }

    public static Sprite GetFlowerBottomSprite(FlowerType t)
    {
        return Instance.flowerBottomSprites[(int)t];
    }

    /// <summary>
    /// Resets the current flower and order to -1 after the minigames all end for the demo
    /// </summary>
    public void ResetFlowerShop()
    {
        currentFlower = -1;
        currentOrder = -1;
    }

    void Start()
    {
        Instance = this;
        NextMinigame();
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
    }

    /// <summary>
    /// Changes the state of the flower shop memory
    /// </summary>
    /// <param name="state">The enum state to change to</param>
    public override void NextMinigame()
    {
        Debug.Log("Minigame: " + currentMinigame);

        if (currentMinigame < 0 || minigames[currentMinigame].GetType() != typeof(InteractManager))
        {

            //As long as the current minigame is at a valid index (greater than 0), stop the minigame at that index
            if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();
                


                //Increment to the next minigame
                currentMinigame++;

            //If the current minigame is higher or equal to the number of minigames, continue
            if (currentMinigame >= minigames.Count)
            {
                CutManager cm = gameObject.AddComponent(typeof(CutManager)) as CutManager;
                cm.ResetCutManager();
                ResetFlowerShop();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

                return;

                // TODO: We're done, end the game (or memory)!!!
            }

            //Start the next minigame
            minigames[currentMinigame].StartMinigame();
        }
       
    }

    /// <summary>
    /// Same as the top one, but actives via the interactable dialogue minigame because the 
    /// dialogue would automatically go to the next scene, which can't happen in minigames
    /// with multiple objects that have dialogue.
    /// </summary>
public void InteractableDialogueNextMinigame()
{

        //As long as the current minigame is at a valid index (greater than 0), stop the minigame at that index
        if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();



        //Increment to the next minigame
        currentMinigame++;

        //If the current minigame is higher or equal to the number of minigames, continue
        if (currentMinigame >= minigames.Count)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;

            // TODO: We're done, end the game (or memory)!!!
        }

        //Start the next minigame
        minigames[currentMinigame].StartMinigame();
    }
}