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
        Sprite[] charSprites = FlowerShopManager.NextOrderChar();
        characterPortrait.GetComponent<SpriteRenderer>().sprite = charSprites[0];
        if (charSprites.Length > 1)
        {
            characterPortrait2.GetComponent<SpriteRenderer>().sprite = charSprites[1];
        }
        gameObject.SetActive(true);
        FlowerShopManager.Instance.NextMinigame();
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }
}
