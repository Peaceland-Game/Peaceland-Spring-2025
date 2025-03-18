using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Keep track of which object needs to be displayed and cut
/// </summary>
public class CutManager : MinigameBehavior
{
    private int curIndex = 0;

    private static CutManager instance;

    [SerializeField]
    private List<GameObject> flowers;

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
        int index = Random.Range(0, instance.flowers.Count - 1);
        instantiatedFlower = Instantiate(instance.flowers[index]);
    }

    
    public static IEnumerator AllCutsMade()
    {
        yield return new WaitForSeconds(1);
        Destroy(instantiatedFlower);
        instance.curIndex++;
       
        if (instance.curIndex < FlowerShopManager.GetCurrentOrder().flowers.Count)
        {
            CutStart();
        }
        else
        {
            FlowerShopManager.Instance.NextMinigame();
        }
    }
}
