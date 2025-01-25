using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    // Update is called once per frame
    void Update()
    {
        //follow the player on the x and y axis
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
    }
}
