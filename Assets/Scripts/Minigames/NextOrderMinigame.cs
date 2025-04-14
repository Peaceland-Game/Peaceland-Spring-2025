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
    private Sprite andrej;
    [SerializeField]
    private Sprite oldFriend;
    [SerializeField]
    private GameObject characterPortrait;

    public override void StartMinigame()
    {
        FlowerShopManager.NextOrder();
        gameObject.SetActive(true);
        FlowerShopManager.Instance.NextMinigame();

        // TODO: DELETE THIS. Hack for playtest.
        characterPortrait.GetComponent<SpriteRenderer>().sprite = oldFriend;
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }
}
