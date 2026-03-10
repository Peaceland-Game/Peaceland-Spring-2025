using UnityEngine;
using UnityEngine.UI; 

public class PieceButtonBehavior : MonoBehaviour{
    public GameObject piece;

    public void SpawnPiece()
    {
        piece.SetActive(true);
        // GameObject piece = Instantiate(piecePrefab);
        Debug.Log("Piece spawned");
    }
}
