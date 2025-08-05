using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiMovePlate : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback

{
    public bool isCastling = false;

    public GameObject controller; //controller는 게임컨트롤러와 상호작용하기 위한 게임오브젝트 필드입니다

    GameObject reference = null; //reference는 이 코드가 할당된 moveplate가 어떤 체스말의 이동경로를 나타내는지를 참조하기위한 변수입니다.

    //matrixX, matrixY는 해당 moveplate의 체스보드상의 좌표를 나타내기 위한 변수입니다.
    int matrixX;
    int matrixY;

    // attack은 해당 이동경로에 공격이 가능한 말인지를 구분하기 위한 부울 자료형입니다.
    public bool attack = false;

    public GameObject ChessMan;

    private bool isMine;



    public void Start() //Start()는 오브젝트가 생성되면 실행되는 메소드입니다. 만약 해당 위치가 공격 가능한 위치일 경우 moveplate의 색상을 붉게 만듭니다. 
    {
        isMine = this.GetComponent<PhotonView>().IsMine;


        if (!isMine)
        {
            Destroy(GetComponent<SpriteRenderer>());
            Destroy(GetComponent<BoxCollider2D>());
        }
        if (attack)
        {
            // Change to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    PhotonView pv;
    void Awake()
    {

       
        // plate생성되면 controller가 비어 있으면 씬에서 GameController 태그로 찾아서 할당
        if (controller == null)
            controller = GameObject.FindGameObjectWithTag("GameController");
    }

    public void OnMouseUp() //실질적으로 이동을 담당하는 함수 
    {
        //GameObject movePiece = reference;
        pv = this.GetComponent<PhotonView>();
        var game = controller.GetComponent<MultiGame>();

        if (isCastling)
        {
            //HandleCastling();    // 킹과 룩을 동시에 이동시키는 로직
            pv.RPC("HandleCastling", RpcTarget.All);
        }
        else
        {
            int attackerID = reference.GetComponent<PhotonView>().ViewID;



            // 캡처 대상이 있으면 DestroySelf RPC 호출
            if (attack)
            {
                GameObject cp = game.GetPosition(matrixX, matrixY);
                if (cp != null)
                {
                    int capturedID = cp.GetComponent<PhotonView>().ViewID;
                    PhotonView.Find(capturedID)
                              .RPC("DestroySelf", RpcTarget.AllBuffered);
                }
            }

            // 이동 RPC 호출
            PhotonView.Find(attackerID)
                      .RPC("MoveTo", RpcTarget.AllBuffered, matrixX, matrixY);
            NormalMove();
        }
        game.CallNextTurn();
    }
    [PunRPC] //동시에 진행되어야하는 함수
    private void HandleCastling()
    {   //viewid를 통해 상대방도 같은 reference를 참조하도록 
        int targetViewID = reference.GetComponent<PhotonView>().ViewID;
        reference = PhotonView.Find(targetViewID)?.gameObject;

        // 1) 왕 이동 (reference는 King GameObject)
        MultiChessMan kingCm = reference.GetComponent<MultiChessMan>();
        MultiGame game = controller.GetComponent<MultiGame>();

        int oldKingX = kingCm.GetXBoard();
        int targetKingX = matrixX;  // 이미 CastlingPlateSpawn 시 xBoard±2 로 세팅됨
        int y = kingCm.GetYBoard();

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

        MultiChessMan[] allPieces = GameObject.FindObjectsByType<MultiChessMan>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        GameObject rookObj = null;

        foreach (MultiChessMan cm in allPieces)
        {
            if (cm.GetPlayer() == kingCm.GetPlayer()      // 같은 플레이어 색
                && cm.GetXBoard() == rookOldX             // 위치가 a-file 또는 h-file
                && cm.GetYBoard() == y                     // 같은 랭크(행)
                && cm.name.Contains("rook"))               // 이름에 rook 포함 (또는 다른 방법으로 룩임을 구분)
            {
                rookObj = cm.gameObject;
                break;
            }
        }
        if (rookObj != null && rookObj.GetComponent<MultiChessMan>().GetPlayer() == kingCm.GetPlayer())
        {
            game.SetPositionEmpty(rookOldX, y); //이전 룩의 위치 비우기 
            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>(); //컴포넌트 갖고 옴
            rookCm.SetXBoard(rookNewX); //캐슬링 규칙에 따라 룩의 위치를 변경(setXBoard, SetCoords)
            rookCm.SetCoords();
            game.SetPosition(rookObj); //2D배열에 룩의 새 위치 반영
        }

        // 3) 턴 넘기기, 플레이트 정리 등
        game.CallNextTurn();
        kingCm.DestroyMovePlates();
        reference.GetComponent<MultiChessMan>().DisableCastling();
    }
    

    [PunRPC] //동시에 진행되어야하는 함수
    public void NormalMove()
    {
        Debug.Log("Normal들어옴");
        
        int PiecetargetViewID = reference.GetComponent<PhotonView>().ViewID;
        reference = PhotonView.Find(PiecetargetViewID)?.gameObject;


        // pawnNeverMove 해제
        if (reference.name.Contains("pawn"))
        {
            reference.GetComponent<MultiChessMan>().DisableDoubleMove();
            reference.GetComponent<MultiChessMan>().DestroyMovePlates();
            int promotionY = reference.GetComponent<MultiChessMan>().GetPlayer() == "white" ? 2 : 0;
            if (matrixY == promotionY)
            {
                int pawnID = reference.GetComponent<PhotonView>().ViewID;
                controller.GetComponent<PhotonView>()
                          .RPC("RPC_ShowPromotionUI",
                               reference.GetComponent<PhotonView>().Owner,
                               pawnID);
                return; // 프로모션의 경우 여기서 멈춤
            }
        }

        if (reference.name.Contains("king"))
        {
            reference.GetComponent<MultiChessMan>().DisableCastling();
        }

        //controller.GetComponent<MultiGame>().SetPosition(reference);
        //controller.GetComponent<MultiGame>().CallNextTurn();
        reference.GetComponent<MultiChessMan>().DestroyMovePlates();
    }



    //SetCoords(int x, int y)는 moveplate의 좌표를 입력하기 위한 메소드입니다. 
    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }
   
    //SetReference(GameObject obj)는 moveplate가 어떤 체스말의 이동경로를 나타내는지를 참조하는 reference 변수를 입력하기 위한 메소드 입니다.
    [PunRPC]
    public void SetReference(GameObject obj)
    {
        reference = obj;

        int viewID = obj.GetComponent<PhotonView>().ViewID;
        photonView.RPC("SyncReference", RpcTarget.Others, viewID);
    }

    [PunRPC] //reference동기화를 위한 함수 
    public void SyncReference(int viewID)
    {
        GameObject target = PhotonView.Find(viewID)?.gameObject;
        if (target != null)
        {
            reference = target;
        }
        else
        {
            Debug.LogWarning("MovePlate: reference 동기화 실패 (viewID: " + viewID + ")");
        }
    }

    //위 두 메소드는 moveplate를 생성할 때 자주 사용하게 됩니다.

    //GetReference()는 다른 코드로 reference를 반환하기 위해 사용하는 메소드입니다.
    public GameObject GetReference()
    {
        return reference;
    }

    //이동시 좌표값 넘기기 
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = photonView.InstantiationData;
        if (instantiationData != null && instantiationData.Length >= 2)
        {
            int x = (int)instantiationData[0];
            int y = (int)instantiationData[1];
            SetCoords(x, y);
        }
    }
}
