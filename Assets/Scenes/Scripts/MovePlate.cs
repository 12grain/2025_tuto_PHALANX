using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public bool isCastling = false;

    public GameObject controller; //controller�� ������Ʈ�ѷ��� ��ȣ�ۿ��ϱ� ���� ���ӿ�����Ʈ �ʵ��Դϴ�

    GameObject reference = null; //reference�� �� �ڵ尡 �Ҵ�� moveplate�� � ü������ �̵���θ� ��Ÿ�������� �����ϱ����� �����Դϴ�.

    //matrixX, matrixY�� �ش� moveplate�� ü��������� ��ǥ�� ��Ÿ���� ���� �����Դϴ�.
    int matrixX;
    int matrixY;

    // attack�� �ش� �̵���ο� ������ ������ �������� �����ϱ� ���� �ο� �ڷ����Դϴ�.
    public bool attack = false;

    public void Start() //Start()�� ������Ʈ�� �����Ǹ� ����Ǵ� �޼ҵ��Դϴ�. ���� �ش� ��ġ�� ���� ������ ��ġ�� ��� moveplate�� ������ �Ӱ� ����ϴ�. 
    {
        if (attack)
        {
            // Change to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }
    
    
    void Awake()
    {
        // plate�����Ǹ� controller�� ��� ������ ������ GameController �±׷� ã�Ƽ� �Ҵ�
        if (controller == null)
            controller = GameObject.FindGameObjectWithTag("GameController");
    }


    public void OnMouseUp()
    {
        if (isCastling)
        {
            HandleCastling();    // ŷ�� ���� ���ÿ� �̵���Ű�� ����
        }
        else
        {
            NormalMove();        // ���� �̵� ����
        }
    }

    private void HandleCastling()
    {
        // 1) �� �̵� (reference�� King GameObject)
        Chessman kingCm = reference.GetComponent<Chessman>();
        Game game = controller.GetComponent<Game>();

        int oldKingX = kingCm.GetXBoard();
        int targetKingX = matrixX;  // �̹� CastlingPlateSpawn �� xBoard��2 �� ���õ�

        // �� ĭ���� SetPositionEmpty, SetPosition �� �⺻ ����
        game.SetPositionEmpty(oldKingX, kingCm.GetYBoard());
        kingCm.SetXBoard(targetKingX);
        kingCm.SetCoords();
        game.SetPosition(reference);

        // 2) �� �̵�
        // �����̵�� a-file(0) ����, ŷ���̵�� h-file(7) ���� ã��
        bool isKingSide = targetKingX > oldKingX;
        int rookOldX = isKingSide ? 7 : 0;
        int rookNewX = isKingSide ? targetKingX - 1 : targetKingX + 1;
        int y = kingCm.GetYBoard();

        //rookObj�� afile ���̳� h-file���� �Ҵ�(getpositionȰ��)
        GameObject rookObj = game.GetPosition(rookOldX, y);

        //���� �����ϰ�, �� ���� ŷ�� ���� �÷��̾�(����)�� ���� ĳ���� �� ���� �̵��� �����ϴ� �ڵ�
        if (rookObj != null && rookObj.GetComponent<Chessman>().GetPlayer() == kingCm.GetPlayer())
        {
            game.SetPositionEmpty(rookOldX, y); //���� ���� ��ġ ���� 
            Chessman rookCm = rookObj.GetComponent<Chessman>(); //������Ʈ ���� ��
            rookCm.SetXBoard(rookNewX); //ĳ���� ��Ģ�� ���� ���� ��ġ�� ����(setXBoard, SetCoords)
            rookCm.SetCoords();
            game.SetPosition(rookObj); //2D�迭�� ���� �� ��ġ �ݿ�
        }

        // 3) �� �ѱ��, �÷���Ʈ ���� ��
        game.NextTurn();
        kingCm.DestroyMovePlates();
        reference.GetComponent<Chessman>().DisableCastling();
    }

    private void Promotion()
    {
        Chessman pawnCm = reference.GetComponent<Chessman>();

        // deprecated: FindObjectOfType<PromotionManager>()
        var promoMgr =
            Object.FindFirstObjectByType<PromotionManager>();
        // �Ǵ�: Object.FindAnyObjectByType<PromotionManager>();

        if (promoMgr != null)
            promoMgr.ShowPromotionUI(pawnCm);
        else
            Debug.LogError("PromotionManager�� ���� �����ϴ�!");

        Destroy(gameObject);
    }

    public void NormalMove()
    {

        // ���� MovePlate�� ���, �ش� ��ġ�� �⹰�� ����
        if (attack)
        {
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);
            Destroy(cp);
        }

        // ���� ��ġ ����
        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(), reference.GetComponent<Chessman>().GetYBoard());

        // ���ο� ��ġ ����
        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords();

        // pawnNeverMove ����
        if (reference.name.Contains("pawn"))
        {
            reference.GetComponent<Chessman>().DisableDoubleMove();

            int promotionY = reference.GetComponent<Chessman>().GetPlayer() == "white" ? 2 : 0;
            if (matrixY == promotionY)
            {
                Promotion();  // ���θ��
                return; // �� �ѱ��� �ʰ� ������ ���� ����
            }
        }

        if (reference.name.Contains("king"))
        {
            reference.GetComponent<Chessman>().DisableCastling();
        }

        controller.GetComponent<Game>().SetPosition(reference);
        controller.GetComponent<Game>().NextTurn();

        reference.GetComponent<Chessman>().DestroyMovePlates();
    }
    //SetCoords(int x, int y)�� moveplate�� ��ǥ�� �Է��ϱ� ���� �޼ҵ��Դϴ�. 
    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }
    //SetReference(GameObject obj)�� moveplate�� � ü������ �̵���θ� ��Ÿ�������� �����ϴ� reference ������ �Է��ϱ� ���� �޼ҵ� �Դϴ�.
    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

  

    //�� �� �޼ҵ�� moveplate�� ������ �� ���� ����ϰ� �˴ϴ�.

    //GetReference()�� �ٸ� �ڵ�� reference�� ��ȯ�ϱ� ���� ����ϴ� �޼ҵ��Դϴ�.
    public GameObject GetReference()
    {
        return reference;
    }
}