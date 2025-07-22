using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    [SerializeField]
    private float transitionTime = 5f;

    void Start()
    {
        Invoke("StartGame", transitionTime);
    }

    void StartGame()
    {
        SceneManager.LoadScene(0);
        //FlowerShopManager.ResetFlowerShop(); error message, doesn't run anyway
    }
}
