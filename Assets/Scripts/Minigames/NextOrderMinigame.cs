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
    private float pauseTimer;

    public override void StartMinigame()
    {
        pauseTimer = 2;
        characterPortrait.SetActive(false);


    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }

    //Added an update method to run a timer in between characters
    public void Update()
    {
        if(pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            if(pauseTimer < 0)
            {
                characterPortrait.SetActive(true);
                characterPortrait.GetComponent<SpriteRenderer>().sprite = FlowerShopManager.NextOrderChar();
                gameObject.SetActive(true);
                FlowerShopManager.Instance.NextMinigame();
            }
        }   
    }
}
