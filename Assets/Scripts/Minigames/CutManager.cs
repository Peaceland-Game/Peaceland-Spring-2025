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
            FlowerShopManager.GetCurrentMinigame()));
    }


    public static IEnumerator AllCutsMade()
    {
        yield return new WaitForSeconds(1);
        Destroy(instantiatedFlower);
        curIndex++;
        FlowerShopManager.currentFlower = curIndex;

        if (curIndex < FlowerShopManager.GetCurrentOrder().flowers.Count)
        {
            if ((FlowerShopManager.GetCurrentFlower(curIndex).needsDethorning && FlowerShopManager.GetCurrentMinigame() == 1) ||
            (FlowerShopManager.GetCurrentFlower(curIndex).needsTrimming && FlowerShopManager.GetCurrentMinigame() == 2))
            {
                CutStart();
            }
            else
            {
                curIndex = 0;
                FlowerShopManager.currentFlower = curIndex;
                FlowerShopManager.Instance.NextMinigame();
            }
        }
        else
        {
            curIndex = 0;
            FlowerShopManager.currentFlower = curIndex;
            FlowerShopManager.Instance.NextMinigame();
        }
    }
}
