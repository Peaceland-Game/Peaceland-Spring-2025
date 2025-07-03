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
    private GameObject characterPortrait; //To swap characters

    [SerializeField]
    private GameObject flowershopBG;

    [SerializeField]
    private Sprite[] flowershopBGsprites; //To close and open flower shop door

    private float pauseTimer;

    [SerializeField]
    private GameObject characterPortrait2;

    public override void StartMinigame()
    {
        FlowerShopManager.NextOrder();
        characterPortrait.GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetMainSprites()[0];
        if (FlowerShopManager.GetSecondSprites() != null)
        {
            characterPortrait2.GetComponent<SpriteRenderer>().sprite = FlowerShopManager.GetSecondSprites()[0];
        }
        pauseTimer = 2;
        characterPortrait.SetActive(false);
        gameObject.SetActive(true);
        flowershopBG.GetComponent<SpriteRenderer>().sprite = flowershopBGsprites[1];
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }

    //Added an update method to run a timer in between characters
    public void Update()
    {
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                //characterPortrait.SetActive(true);
                flowershopBG.GetComponent<SpriteRenderer>().sprite = flowershopBGsprites[0];
                FlowerShopManager.Instance.NextMinigame();
            }
        }
    }
}


