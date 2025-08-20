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
                Debug.Log("��Ȳ 1");
                HandlePieceSelection();
            }
            else
            {
                Debug.Log("��Ȳ 2");
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
        // ���콺 ��ġ�� 2D ���� ��ǥ�� ��ȯ
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2D Raycast�� ����Ͽ� "Piece" ���̾ ����
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f, pieceLayerMask);

        if (hit.collider != null)
        {
            Debug.Log(hit.collider.name + " �⹰�� ���õǾ����ϴ�.");
            selectedPiece = hit.collider.gameObject;
            isPieceSelected = true;

            // ���⿡ Plate���� �����ϰ� �����ִ� ������ �����մϴ�.
            // ShowPlatesFor(selectedPiece);
        }
    }

    void HandlePlateSelection()
    {
        // ���콺 ��ġ�� 2D ���� ��ǥ�� ��ȯ
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2D Raycast�� ����Ͽ� "Plate" ���̾ ����
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f, plateLayerMask);

        if (hit.collider != null)
        {
            Debug.Log(hit.collider.name + " Plate�� Ŭ���Ǿ����ϴ�.");

            // ���⿡ �⹰�� �ش� Plate ��ġ�� �̵���Ű�� ������ �����մϴ�.
            // MovePieceTo(selectedPiece, hit.transform.position);

            DeselectPiece();
        }
    }

    void DeselectPiece()
    {
        if (isPieceSelected)
        {
            Debug.Log("�⹰ ������ ��ҵǾ����ϴ�.");
            isPieceSelected = false;
            selectedPiece = null;

            // ���⿡ ��� Plate���� ����ų� �����ϴ� ������ �����մϴ�.
            // HideAllPlates();
        }
    }
}