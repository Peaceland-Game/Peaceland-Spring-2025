using System.Collections.Generic;
using UnityEngine;

public class Stem : MonoBehaviour
{
    [SerializeField] GameObject thornPrefab;
    [SerializeField] LineRenderer cutLine;

    List<GameObject> thorns = new List<GameObject>();

    [SerializeField] int minThorns;
    [SerializeField] int maxThorns;

    [SerializeField] int leftXSpawnPos;
    [SerializeField] int rightXSpawnPos;

    [SerializeField] float minSpawnHeight;
    [SerializeField] float maxSpawnHeight;

    int thornCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thornCount = Random.Range(minThorns, maxThorns);
        for (int i = 0; i < thornCount; i++)
        {
            //spawn thorn
            GameObject newThorn = Instantiate(thornPrefab);
            thorns.Add(newThorn);
            newThorn.GetComponent<CutLogic>().AssignValues(cutLine, this);


            float thornY = Random.Range(minSpawnHeight, maxSpawnHeight);
            if (Random.Range(0, 2) == 0)
            {
                newThorn.transform.position = new Vector3(leftXSpawnPos, thornY, 0);
            }
            else
            {
                newThorn.transform.position = new Vector3(rightXSpawnPos, thornY, 0);
                newThorn.transform.localScale = new Vector3(newThorn.transform.localScale.x * -1, newThorn.transform.localScale.y, newThorn.transform.localScale.z);
            }
        }
    }

    public void CutMade(GameObject cutThorn)
    {
        thorns.Remove(cutThorn);
        Debug.Log(thorns.Count);
        if (thorns.Count == 0)
        {
            StartCoroutine(CutManager.AllCutsMade());
        }
    }
}
