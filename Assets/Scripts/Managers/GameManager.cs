using UnityEngine;

//data that needs to be stored globally and transfered between scenes can be stored and referenced in this script
public class GameManager : MonoBehaviour
{
    //bools for the MuseumIntro scene, used in MuseumIntroManager.cs and SceneTransitioner.cs
    public bool newsRead;
    public bool introSprawlDone;
    public bool marcStart;
    public bool miraIntroDone;
    public bool memoryObjectAcquired;

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
    void Start()
    {
        _instance = this;
        DontDestroyOnLoad(_instance);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
