using System.Collections;
using UnityEngine;

public class CutPiece : MonoBehaviour
{
    /// <summary>
    /// moves the object down, then to the right
    /// </summary>
    /// <returns></returns>
    public IEnumerator MoveSnippedObject(float downTime, float rightTime, float speedMultiStart)
    {
        //move down
        float startTime = Time.time;
        float t = 0;
        float speedMultiplier = speedMultiStart;
        Vector3 startPos = transform.position;
        Vector3 endPos = new(startPos.x, -4, startPos.z);
        while (t < 1)
        {
            t = (Time.time - startTime) / downTime;
            transform.position = Vector3.Lerp(startPos, endPos, t * speedMultiplier);
            speedMultiplier = Mathf.Lerp(1.5f, 1f, t);
            yield return new WaitForEndOfFrame();
        }

        //move to the right
        startTime = Time.time;
        t = 0;
        startPos = transform.position;
        speedMultiplier = speedMultiStart;
        endPos = new(10, startPos.y, startPos.z);
        while (t < 1)
        {
            t = (Time.time - startTime) / rightTime;
            transform.position = Vector3.Lerp(startPos, endPos, t * speedMultiplier);
            speedMultiplier = Mathf.Lerp(1.5f, 1f, t);
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
}
