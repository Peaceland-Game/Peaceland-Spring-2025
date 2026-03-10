using UnityEngine;
using UnityEngine.UI; 

public class PieceButtonBehavior : MonoBehaviour{
    public GameObject piece;

    public void SpawnPiece()
    {
        if (piece.activeSelf){
            piece.SetActive(false);
        } else {  
            piece.SetActive(true); 
        }
        // GameObject piece = Instantiate(piecePrefab);
        //Debug.Log("Piece spawned");
    }
}
