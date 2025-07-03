using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;
using Unity.VisualScripting;
using System;

/// <summary>
/// Keep track of which object needs to be displayed and cut
/// </summary>
public class CutManager : MinigameBehavior
{
    private static int curIndex = 0;

    public static int CurIndex
    {
        get { return curIndex; }
        set { curIndex = value; }
    }

    private static CutManager instance;

    private static GameObject instantiatedFlower;

    public GameObject thornyRose;

    /// <summary>
    /// Stem for dethorning minigame
    /// </summary>
    public GameObject stem;

    /// <summary>
    /// Dynamic sprite cutting flower game object
    /// </summary>
    public GameObject flower;

    private static Boolean isFirstDethorn = true;

    public override void StartMinigame()
    {

        instance = this;
        gameObject.SetActive(true);
        CutStart();

        if (GameManager.Instance.difficulty > 1)
        {
            PostProcessVolume ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
            ppVolume.weight = 1;
            ppVolume.enabled = true;
            if (GameManager.Instance.difficulty >= 2)
            { //Scales from 2 to 11
                ppVolume.weight = 0.45f + (GameManager.Instance.difficulty * 0.05f);
            }
        }
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }

    public static void CutStart()
    {
        //If the current minigame is dethorning, then use the pre-given stem game object
        if (FlowerShopManager.GetCurrentMinigame().gameObject.name == "Dethorn")
        {
            //Activates the thorny rose object for the zoom in on the first rose of the minigame
            if (isFirstDethorn)
            {
                isFirstDethorn = false;
                ThornyFlowerZoom t = instance.thornyRose.GetComponent<ThornyFlowerZoom>();
                t.Reset();
            }
            //Otherwise load in the next thorns normally
            else
            {
                instance.BeginDethorn();
            }
        }
        //Otherwise use the dynamic sprite flower
        else { instantiatedFlower = Instantiate(instance.flower); }
    }

    public void BeginDethorn()
    {
        instantiatedFlower = Instantiate(instance.stem);
    }

    public static IEnumerator AllCutsMade()
    {
        yield return new WaitForSeconds(1);
        Destroy(instantiatedFlower);
        curIndex++;
        FlowerShopManager.currentFlower = curIndex;

        //As long as the next flower in the list for this order exists, continue below
        if (curIndex < FlowerShopManager.GetCurrentOrder().flowers.Count)
        {
            //If the current flower needs to be dethorned and the current minigame is dethorning, or the current flower needs to be
            //trimmed and the current minigame is trimming, then set up the next flower
            if ((FlowerShopManager.GetCurrentFlower(curIndex).needsDethorning && FlowerShopManager.GetCurrentMinigame().gameObject.name == "Dethorn") ||
            (FlowerShopManager.GetCurrentFlower(curIndex).needsTrimming && FlowerShopManager.GetCurrentMinigame().gameObject.name == "Trimming"))
            {
                CutStart();
            }
            //Otherwise if the current flower is the last one in the list, go back to the first flower in the list
            else if (curIndex == FlowerShopManager.GetCurrentOrder().flowers.Count - 1)
            {
                curIndex = 0;

                //Update the current flower in the FlowerShopManager, and move to the next minigame
                FlowerShopManager.currentFlower = curIndex;
                FlowerShopManager.Instance.NextMinigame();
            }
            //And if neither of the statements above are true, then move to the next flower
            else
            {
                curIndex++;

                //Update the current flower in the FlowerShopManager, and set up the next flower
                FlowerShopManager.currentFlower = curIndex;
                CutStart();
            }
        }
        //Otherwise move to the next minigame
        else
        {

            //Remove the blur from minigames with added difficulty
            PostProcessVolume ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
            ppVolume.enabled = false;

            curIndex = 0;
            FlowerShopManager.currentFlower = curIndex;
            FlowerShopManager.Instance.NextMinigame();
        }
    }
}