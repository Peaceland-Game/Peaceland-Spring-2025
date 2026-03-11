using UnityEngine;
using UnityEngine.UI; 

public class PieceButtonBehavior : MonoBehaviour{
    public GameObject piece;

    //might need to use this function to set the piece for each button
    public void setPiece(GameObject _piece){
        piece = _piece;
    }

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
