using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Keep track of which object needs to be displayed and cut
/// </summary>
public class NextOrderMinigame : MinigameBehavior
{
    [SerializeField]
    private GameObject characterPortrait;

    [SerializeField]
    private GameObject characterPortrait2;

    public override void StartMinigame()
    {
        FlowerShopManager.NextOrder();
        characterPortrait.GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetMainSprites()[0];
        if (FlowerShopManager.GetSecondSprites()!=null)
        {
            characterPortrait2.GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetSecondSprites()[0];
        }
        gameObject.SetActive(true);
        FlowerShopManager.Instance.NextMinigame();
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }
}
