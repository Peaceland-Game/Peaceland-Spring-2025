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

    public override void StartMinigame()
    {
        characterPortrait.GetComponent<SpriteRenderer>().sprite = FlowerShopManager.NextOrderChar();
        gameObject.SetActive(true);
        FlowerShopManager.Instance.NextMinigame();
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }
}
