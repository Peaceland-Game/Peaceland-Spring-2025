using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Keep track of which object needs to be displayed and cut
/// </summary>
public class NextOrderMinigame : MinigameBehavior
{
    public override void StartMinigame()
    {
        FlowerShopManager.NextOrder();
        gameObject.SetActive(true);
        FlowerShopManager.Instance.NextMinigame();

        // TODO: DELETE THIS. Hack for playtest.
        GameObject.Find("Andrej").GetComponent<SpriteRenderer>().color = Color.black;
    }

    public override void StopMinigame()
    {
        gameObject.SetActive(false);
    }
}
