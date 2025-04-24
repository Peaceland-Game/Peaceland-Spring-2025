using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    /// <summary>
    /// The amount of time before the demo automatically restarts
    /// </summary>
    [SerializeField]
    private float transitionTime = 5f;

    void Start()
    {
        //Start the countdown to restarting the demo
        Invoke("StartGame", transitionTime);
    }

    void StartGame()
    {
        //Load the start of the demo, and make sure to reset any static vars in the flowershop memory
        FlowerShopManager.ResetFlowerShop();
        SceneManager.LoadScene(0);
    }
}
