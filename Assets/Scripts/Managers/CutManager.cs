using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Keep track of which object needs to be displayed and cut
/// </summary>
public class CutManager : MonoBehaviour
{
    private int curIndex = 0;

    private static CutManager instance;

    [SerializeField]
    private List<GameObject> flowers;

    [SerializeField]
    private FlowerManager.FlowerGameState nextState;

    private static GameObject instantiatedFlower;

    private void Start()
    {
        instance = this;
        CutStart();
    }

    public static void CutStart()
    {
        int index = Random.Range(0, instance.flowers.Count - 1);
        instantiatedFlower = Instantiate(instance.flowers[index]);
    }

    public static void CutMade()
    {
        Destroy(instantiatedFlower);
        instance.curIndex++;
       
        if (instance.curIndex < FlowerManager.GetCurrentOrder().flowers.Count)
        {
            CutStart();
        }
        else
        {
            FlowerManager.Instance.ChangeState(instance.nextState);
        }
    }
}
