using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public LayerMask pieceLayerMask;
    public LayerMask plateLayerMask;

    private bool isPieceSelected = false;
    private GameObject selectedPiece = null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isPieceSelected == false)
            {
                Debug.Log("상황 1");
                HandlePieceSelection();
            }
            else
            {
                Debug.Log("상황 2");
                HandlePlateSelection();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            DeselectPiece();
        }
    }

    void HandlePieceSelection()
    {
        // 마우스 위치를 2D 월드 좌표로 변환
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2D Raycast를 사용하여 "Piece" 레이어만 감지
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f, pieceLayerMask);

        if (hit.collider != null)
        {
            Debug.Log(hit.collider.name + " 기물이 선택되었습니다.");
            selectedPiece = hit.collider.gameObject;
            isPieceSelected = true;

            // 여기에 Plate들을 생성하고 보여주는 로직을 실행합니다.
            // ShowPlatesFor(selectedPiece);
        }
    }

    void HandlePlateSelection()
    {
        // 마우스 위치를 2D 월드 좌표로 변환
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2D Raycast를 사용하여 "Plate" 레이어만 감지
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f, plateLayerMask);

        if (hit.collider != null)
        {
            Debug.Log(hit.collider.name + " Plate가 클릭되었습니다.");

            // 여기에 기물을 해당 Plate 위치로 이동시키는 로직을 실행합니다.
            // MovePieceTo(selectedPiece, hit.transform.position);

            DeselectPiece();
        }
    }

    void DeselectPiece()
    {
        if (isPieceSelected)
        {
            Debug.Log("기물 선택이 취소되었습니다.");
            isPieceSelected = false;
            selectedPiece = null;

            // 여기에 모든 Plate들을 숨기거나 제거하는 로직을 실행합니다.
            // HideAllPlates();
        }
    }
}