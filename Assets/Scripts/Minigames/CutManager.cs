using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    public override void StartMinigame()
    {
        instance = this;
        gameObject.SetActive(true);
        CutStart();
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }

    public static void CutStart()
    {
        instantiatedFlower = Instantiate(FlowerShopManager.ReturnGameObjectBasedOnMinigame(curIndex,
            FlowerShopManager.GetCurrentMinigame().gameObject.name));
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
            curIndex = 0;
            FlowerShopManager.currentFlower = curIndex;
            FlowerShopManager.Instance.NextMinigame();
        }
    }
}
