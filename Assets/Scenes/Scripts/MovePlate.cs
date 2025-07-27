using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public bool isCastling = false;

    public GameObject controller; //controller는 게임컨트롤러와 상호작용하기 위한 게임오브젝트 필드입니다

    GameObject reference = null; //reference는 이 코드가 할당된 moveplate가 어떤 체스말의 이동경로를 나타내는지를 참조하기위한 변수입니다.

    //matrixX, matrixY는 해당 moveplate의 체스보드상의 좌표를 나타내기 위한 변수입니다.
    int matrixX;
    int matrixY;

    // attack은 해당 이동경로에 공격이 가능한 말인지를 구분하기 위한 부울 자료형입니다.
    public bool attack = false;

    public void Start() //Start()는 오브젝트가 생성되면 실행되는 메소드입니다. 만약 해당 위치가 공격 가능한 위치일 경우 moveplate의 색상을 붉게 만듭니다. 
    {
        if (attack)
        {
            // Change to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }
    
    
    void Awake()
    {
        // plate생성되면 controller가 비어 있으면 씬에서 GameController 태그로 찾아서 할당
        if (controller == null)
            controller = GameObject.FindGameObjectWithTag("GameController");
    }


    public void OnMouseUp()
    {
        if (isCastling)
        {
            HandleCastling();    // 킹과 룩을 동시에 이동시키는 로직
        }
        else
        {
            NormalMove();        // 기존 이동 로직
        }
    }

    private void HandleCastling()
    {
        // 1) 왕 이동 (reference는 King GameObject)
        Chessman kingCm = reference.GetComponent<Chessman>();
        Game game = controller.GetComponent<Game>();

        int oldKingX = kingCm.GetXBoard();
        int targetKingX = matrixX;  // 이미 CastlingPlateSpawn 시 xBoard±2 로 세팅됨

        // 빈 칸으로 SetPositionEmpty, SetPosition 등 기본 동작
        game.SetPositionEmpty(oldKingX, kingCm.GetYBoard());
        kingCm.SetXBoard(targetKingX);
        kingCm.SetCoords();
        game.SetPosition(reference);

        // 2) 룩 이동
        // 퀸사이드면 a-file(0) 룩을, 킹사이드면 h-file(7) 룩을 찾고
        bool isKingSide = targetKingX > oldKingX;
        int rookOldX = isKingSide ? 7 : 0;
        int rookNewX = isKingSide ? targetKingX - 1 : targetKingX + 1;
        int y = kingCm.GetYBoard();

        //rookObj에 afile 룩이나 h-file룩을 할당(getposition활용)
        GameObject rookObj = game.GetPosition(rookOldX, y);

        //룩이 존재하고, 그 룩이 킹과 같은 플레이어(색상)일 때만 캐슬링 용 룩의 이동을 수행하는 코드
        if (rookObj != null && rookObj.GetComponent<Chessman>().GetPlayer() == kingCm.GetPlayer())
        {
            game.SetPositionEmpty(rookOldX, y); //이전 룩의 위치 비우기 
            Chessman rookCm = rookObj.GetComponent<Chessman>(); //컴포넌트 갖고 옴
            rookCm.SetXBoard(rookNewX); //캐슬링 규칙에 따라 룩의 위치를 변경(setXBoard, SetCoords)
            rookCm.SetCoords();
            game.SetPosition(rookObj); //2D배열에 룩의 새 위치 반영
        }

        // 3) 턴 넘기기, 플레이트 정리 등
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
        // 또는: Object.FindAnyObjectByType<PromotionManager>();

        if (promoMgr != null)
            promoMgr.ShowPromotionUI(pawnCm);
        else
            Debug.LogError("PromotionManager가 씬에 없습니다!");

        Destroy(gameObject);
    }

    public void NormalMove()
    {

        // 공격 MovePlate인 경우, 해당 위치의 기물을 제거
        if (attack)
        {
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);
            Destroy(cp);
        }

        // 기존 위치 비우기
        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(), reference.GetComponent<Chessman>().GetYBoard());

        // 새로운 위치 설정
        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords();

        // pawnNeverMove 해제
        if (reference.name.Contains("pawn"))
        {
            reference.GetComponent<Chessman>().DisableDoubleMove();

            int promotionY = reference.GetComponent<Chessman>().GetPlayer() == "white" ? 2 : 0;
            if (matrixY == promotionY)
            {
                Promotion();  // 프로모션
                return; // 턴 넘기지 않고 종료할 수도 있음
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
    //SetCoords(int x, int y)는 moveplate의 좌표를 입력하기 위한 메소드입니다. 
    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }
    //SetReference(GameObject obj)는 moveplate가 어떤 체스말의 이동경로를 나타내는지를 참조하는 reference 변수를 입력하기 위한 메소드 입니다.
    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

  

    //위 두 메소드는 moveplate를 생성할 때 자주 사용하게 됩니다.

    //GetReference()는 다른 코드로 reference를 반환하기 위해 사용하는 메소드입니다.
    public GameObject GetReference()
    {
        return reference;
    }
}