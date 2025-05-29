using System.Collections;
using UnityEngine;

public class CutPiece : MonoBehaviour
{
    /// <summary>
    /// moves the object down towards the trash can and shinks it
    /// </summary>
    /// <returns></returns>
    public IEnumerator MoveSnippedObject(float downTime, float rightTime, float speedMultiStart)
    {
        //Tried to make the clipped thorns go behind the unclipped ones, I think it works. Feel free to comment this out if it's still messing with the stem trimmings.
        transform.position = new(transform.position.x, transform.position.y, transform.position.z + 1f);

        //move down
        float startTime = Time.time;
        float t = 0;
        float speedMultiplier = speedMultiStart;
        Vector3 startPos = transform.position;
        Vector3 endPos = new(startPos.x, -1, startPos.z);
        while (t < 1)
        {
            transform.localScale *= 0.999f;
            t = (Time.time - startTime) / downTime;
            transform.position = Vector3.Lerp(startPos, endPos, t * speedMultiplier);
            speedMultiplier = Mathf.Lerp(1.5f, 1f, t);
            yield return new WaitForEndOfFrame();
        }

        /*
        //old code to move to the right
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
        */
        Destroy(transform.parent.gameObject);
    }
}
