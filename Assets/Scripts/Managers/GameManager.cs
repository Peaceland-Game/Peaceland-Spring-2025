using UnityEngine;

//data that needs to be stored globally and transfered between scenes can be stored and referenced in this script
public class GameManager : MonoBehaviour
{
    //variables, whatever needs to be kept track of between scenes

    //create the private instance
    private static GameManager _instance;

    //create the public reference
    public static GameManager Instance
    {
        get
        {
            if (_instance is null)
                Debug.LogError("GameManager is NULL");

            return _instance;
        }
    }

    //initialize private instance
    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(_instance);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
