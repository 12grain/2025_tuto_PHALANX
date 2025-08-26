using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFlip : MonoBehaviour
{

    MultiGame multiGame;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    MultiChessMan[] ChessPieces;
    bool isBlack;
    IEnumerator Start()
    {
        multiGame = GameObject.FindWithTag("GameController").GetComponent<MultiGame>();

        isBlack =( multiGame.GetMyPlayerColor() == "black");

        if (isBlack)
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        yield return new WaitForSeconds(0.05f);
        ChessPieces = FindObjectsByType<MultiChessMan>(FindObjectsSortMode.None);




        if (isBlack)
        {
            foreach (MultiChessMan chessPiece in ChessPieces)
            {
                chessPiece.gameObject.GetComponent<SpriteRenderer>().flipY = true;

            }
        }
        
        
    }



    void Update()
    {
        if (!isBlack) return; // white면 아무것도 안 함

        // 매 프레임 기물 확인
        ChessPieces = FindObjectsByType<MultiChessMan>(FindObjectsSortMode.None);

        foreach (MultiChessMan piece in ChessPieces)
        {
            // Z축 회전값이 180이 아닌 경우만 돌림 → 불필요한 연산 방지
            float zRotation = Mathf.Round(piece.transform.eulerAngles.z);
            if (zRotation != 180f)
            {
                piece.gameObject.GetComponent<SpriteRenderer>().flipY = true;

            }
        }
    }
    
}

   
  