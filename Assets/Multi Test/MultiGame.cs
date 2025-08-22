using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MultiGame : MonoBehaviourPunCallbacks
{
    public GameObject chesspiece;
    public Sprite boardSprite;
    public Sprite backgroundSprite;
    public PromotionManager promotionManager;

    // Positions and team for each chesspiece
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private string myPlayerColor;
    private string currentPlayer ;

    private bool gameOver = false;
    public bool isInteractionBlocked = false;
    private PhotonView pv;

    public float tileSize = 0.66f;                 // 자동 재계산 되지만 기본값
    public Vector2 boardOrigin = Vector2.zero;     // 0,0 칸(=a1)의 월드 좌표(센터)
    public float boardWorldWidth = 0f;
    public float boardWorldHeight = 0f;

    //private bool turnChanged = false;


    void Awake()
    {
        myPlayerColor = PlayerPrefs.GetString("MyColor"); 
        Debug.Log("내 플레이어 색은: " + myPlayerColor);
    }

    void Start()
    {

        //배경 배치
        GameObject background = new GameObject("background");
        background.transform.position = new Vector3(0f, 0f, 0);
        var backgroundSr = background.AddComponent<SpriteRenderer>();
        backgroundSr.sprite = backgroundSprite;
        backgroundSr.sortingLayerName = "Board";
        backgroundSr.sortingOrder = 0;

        //board 배치
        GameObject board = new GameObject("Board");
        board.transform.position = new Vector3(0f, 0f, 0);
        var boardSr = board.AddComponent<SpriteRenderer>();
        boardSr.sprite = boardSprite;
        boardSr.sortingLayerName = "Board";
        boardSr.sortingOrder = 1;

        RecalculateBoardMetrics(); //보드 재배치 (새로운 에셋으로 인한 크기변경)

        currentPlayer = "white";

        if (PhotonNetwork.IsMasterClient)
        {
            playerWhite = new GameObject[] { Create("white_rook", 0, 0), Create("white_knight", 1, 0),
            Create("white_bishop", 2, 0), Create("white_queen", 3, 0), Create("white_king", 4, 0),
            Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1),
            Create("white_pawn", 3, 1), Create("white_pawn", 4, 1), Create("white_pawn", 5, 1),
            Create("white_pawn", 6, 1), Create("white_pawn", 7, 1) };

            playerBlack = new GameObject[] { Create("black_rook", 0, 7), Create("black_knight", 1, 7),
            Create("black_bishop", 2, 7), Create("black_queen", 3, 7), Create("black_king", 4, 7),
            Create("black_bishop", 5, 7), Create("black_knight", 6, 7), Create("black_rook", 7, 7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6),
            Create("black_pawn", 3, 6), Create("black_pawn", 4, 6), Create("black_pawn", 5, 6),
            Create("black_pawn", 6, 6), Create("black_pawn", 7, 6) };
        

        // Set all piece positions on the position board
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
        
        }
    }

    public void RecalculateBoardMetrics()
    {
        GameObject boardObj = GameObject.Find("Board");
        if (boardObj == null) return;

        var sr = boardObj.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        // 보드의 실제 월드 사이즈
        boardWorldWidth = sr.bounds.size.x;
        boardWorldHeight = sr.bounds.size.y;

        // 한 칸 크기
        tileSize = boardWorldWidth / 8f;

        // (0,0) 칸의 CENTER 좌표 계산:
        // boardObj.transform.position 은 보드의 중심(가정). 
        // 왼쪽 하단 모서리 = center - (width/2, height/2)
        float left = boardObj.transform.position.x - boardWorldWidth / 2f;
        float bottom = boardObj.transform.position.y - boardWorldHeight / 2f;

        // (0,0) 타일의 center = left + tileSize/2, bottom + tileSize/2
        boardOrigin = new Vector2(left + tileSize * 0.5f, bottom + tileSize * 0.5f);

        Debug.Log($"Recalc: boardW={boardWorldWidth}, tileSize={tileSize}, boardOrigin={boardOrigin}");
    }

    // MultiGame.cs

    public GameObject Create(string name, int x, int y)
    {
        // 1. 요청받은 이름("white_CHEVALIER")에서 순수 기물 타입("CHEVALIER")을 추출합니다.
        string pieceType = name.Split('_')[1];
        string prefabToInstantiate;

        // 2. 기물 타입이 '합성 기물' 중 하나인지 확인합니다.
        if (pieceType == "PHALANX" || pieceType == "TESTUDO" || pieceType == "CHEVALIER" || pieceType == "BASTION")
        {
            // 2-1. 합성 기물이라면, 요청받은 이름 그대로의 프리팹을 사용합니다.
            // 예: "white_CHEVALIER" -> "white_CHEVALIER.prefab"
            prefabToInstantiate = name;
        }
        else
        {
            // 2-2. 그 외의 모든 일반 기물(rook, pawn 등)이라면, 기존의 기본 프리팹을 사용합니다.
            prefabToInstantiate = "MultiChesspiece";
        }

        // 3. 결정된 프리팹 이름으로 경로를 만들고, 올바른 프리팹을 생성합니다.
        string prefabPath = "Pieces/" + prefabToInstantiate;
        Vector3 spawnPos = new Vector3(0f, 0f, 0);
        GameObject obj = PhotonNetwork.Instantiate(prefabPath, spawnPos, Quaternion.identity);

        // 4. 생성된 객체의 세부 설정을 위해 RPC를 호출합니다. (이 부분은 동일)
        PhotonView pv = obj.GetComponent<PhotonView>();
        pv.RPC("SetupSprite", RpcTarget.AllBuffered, name, x, y);

        return obj;
    }

    // 이걸로 보드 위치 변경
    public void SetPosition(GameObject obj)
    {
        MultiChessMan cm = obj.GetComponent<MultiChessMan>();

        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public bool checkLeftSide(int x, int y)
    {
        if (positions[x - 1, y] == null && positions[x - 2, y] == null && positions[x - 3, y] == null) return true;
        return false;
    }
    public bool checkRightSide(int x, int y)
    {
        if (positions[x + 1, y] == null && positions[x + 2, y] == null) return true;
        return false;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool PositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1)) return false;
        return true;
    }

    //GetCurrentPlayer()은 현재 플레이어를 나타내기위한 string 변수 currentPlayer를 반환합니다.
    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public string GetMyPlayerColor() {
    return myPlayerColor;
    }
    //IsGameOver()은 체크메이트상태(게임종료상태)인지를 나타내기 위한 bool 변수 gameOver를 반환합니다.
    public bool IsGameOver()
    {
        return gameOver;
    }

    //NextTurn()은 현재가 백의 차례이면 흑에게, 흑의 차례이면 백에게 넘기는 알고리즘을 구현합니다.
    [PunRPC]//동시에 구현되어야 할 함수
    public void NextTurn() 
    {
        
        if (currentPlayer == "white")
        {
            currentPlayer = "black";
        }
        else
        {
            currentPlayer = "white";
        }
        Debug.Log("턴 넘김");
    }
   

    public void CallNextTurn()
    {
        photonView.RPC("NextTurn", RpcTarget.AllBuffered);
    }

    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject mp in movePlates)
        {
            Destroy(mp);
        }
    }

    // MultiGame.cs

    // MultiGame.cs 의 RequestMovePiece 함수
    [PunRPC]
    public void RequestMovePiece(int attackerID, int targetX, int targetY, int capturedID, bool isCastle)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView attackerView = PhotonView.Find(attackerID);
        if (attackerView == null) return;
        MultiChessMan attackerCm = attackerView.GetComponent<MultiChessMan>();

        // ▼▼▼ 주둔 해제 로직을 여기서 먼저 처리합니다 ▼▼▼
        bool wasGarrisoned = attackerCm.IsGarrisoned();
        int bastionToReleaseID = -1;

        if (wasGarrisoned)
        {
            bastionToReleaseID = attackerCm.GetGarrisonedBastionID();
            PhotonView bastionView = PhotonView.Find(bastionToReleaseID);

            if (bastionView != null)
            {
                bastionView.RPC("RPC_SetVisible", RpcTarget.All, true);
            }

            attackerView.RPC("RPC_SetGarrisonStatus", RpcTarget.All, false, -1);
        }

        // --- 일반 이동/캐슬링 처리 ---
        if (isCastle)
        {
            photonView.RPC("RPC_ExecuteCastling", RpcTarget.All, attackerID, targetX);
        }
        else
        {
            if (capturedID != -1)
            {
                PhotonView.Find(capturedID)?.RPC("DestroySelf", RpcTarget.AllBuffered);
            }

            bool isCapture = (capturedID != -1);
            attackerView.RPC("RPC_AnimateMove", RpcTarget.All, targetX, targetY, isCapture);
        }

        // ▼▼▼ 주둔 해제 후속 처리 ▼▼▼
        // 만약 기물이 바스티온에서 떠난 것이었다면,
        if (wasGarrisoned && bastionToReleaseID != -1)
        {
            // 모든 클라이언트에게 "그 자리에 바스티온을 다시 놓아라"고 명령
            photonView.RPC("RPC_PlacePieceOnBoard", RpcTarget.All, bastionToReleaseID);
        }

        attackerView.RPC("RPC_UpdateMovedStatus", RpcTarget.All);
        CallNextTurn();
    }
    // 캐슬링 가능 여부를 확인하는 상세 함수
    public bool CanCastle(int kingX, int kingY, bool isKingSide)
    {
        // 1. 킹 사이드(오른쪽) 캐슬링 확인
        if (isKingSide)
        {
            // 1-1. 경로가 비었는지 확인
            if (positions[kingX + 1, kingY] != null || positions[kingX + 2, kingY] != null)
            {
                return false;
            }

            // 1-2. 코너에 룩이 있는지 확인
            GameObject rookObj = positions[kingX + 3, kingY];
            if (rookObj == null || !rookObj.name.Contains("rook"))
            {
                return false;
            }

            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>();
            if (rookCm == null || !rookCm.GetRookNeverMove())
            {
                // 룩의 스크립트가 없거나, GetRookNeverMove()가 false를 반환하면 캐슬링 불가
                return false;
            }
            return true; // 모든 조건을 통과
        }
        // 2. 퀸 사이드(왼쪽) 캐슬링 확인
        else
        {
            // 2-1. 경로가 비었는지 확인
            if (positions[kingX - 1, kingY] != null || positions[kingX - 2, kingY] != null || positions[kingX - 3, kingY] != null)
            {
                return false;
            }

            // 2-2. 코너에 룩이 있는지 확인
            GameObject rookObj = positions[kingX - 4, kingY];
            if (rookObj == null || !rookObj.name.Contains("rook"))
            {
                return false;
            }

            // 2-3. 그 룩이 움직인 적 없는지 확인
            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>();
            if (rookCm == null || !rookCm.GetRookNeverMove())
            {
                return false;
            }
            return true;
        }
    }


    // MultiGame.cs 에 있는 RPC_ExecuteCastling 함수
    [PunRPC]
    public void RPC_ExecuteCastling(int kingID, int kingTargetX)
    {
        GameObject kingObj = PhotonView.Find(kingID).gameObject;
        MultiChessMan kingCm = kingObj.GetComponent<MultiChessMan>();

        int startKingX = kingCm.GetXBoard();
        int y = kingCm.GetYBoard();
        bool isKingSide = kingTargetX > startKingX;

        // 1. 킹 이동
        SetPositionEmpty(startKingX, y);

        // ▼▼▼ 이 부분이 수정되었습니다 ▼▼▼
        kingObj.GetComponent<PhotonView>().RPC("RPC_AnimateMove", RpcTarget.All, kingTargetX, y, false);         // 2. 바뀐 내부 좌표를 보고 실제 위치를 옮기게 한다.

        SetPosition(kingObj);

        // 2. 룩 찾기 및 이동
        int rookStartX = isKingSide ? 7 : 0;
        int rookTargetX = isKingSide ? kingTargetX - 1 : kingTargetX + 1;

        GameObject rookObj = GetPosition(rookStartX, y);
        if (rookObj != null)
        {
            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>();
            SetPositionEmpty(rookStartX, y);

            // ▼▼▼ 룩도 똑같이 수정되었습니다 ▼▼▼
            rookObj.GetComponent<PhotonView>().RPC("RPC_AnimateMove", RpcTarget.All, rookTargetX, y, false);

            SetPosition(rookObj);
        }
    }

    // MultiGame.cs 에 추가 (기존 RequestPawnMoveAndShowPromotionUI는 삭제)

    [PunRPC]
    public void RPC_ExecutePromotion(int pawnViewID, int targetX, int targetY, int capturedID, string pieceType)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 1. 잡을 말이 있으면 먼저 파괴
        if (capturedID != -1)
        {
            PhotonView.Find(capturedID)?.RPC("DestroySelf", RpcTarget.AllBuffered);
        }

        // 2. 프로모션할 폰의 정보를 가져온 뒤 파괴
        GameObject pawnObj = PhotonView.Find(pawnViewID)?.gameObject;
        if (pawnObj == null) return;

        MultiChessMan pawnCm = pawnObj.GetComponent<MultiChessMan>();
        int startX = pawnCm.GetXBoard();
        int startY = pawnCm.GetYBoard();
        string playerColor = pawnCm.GetPlayer();

        SetPositionEmpty(startX, startY);
        PhotonNetwork.Destroy(pawnObj);

        // 3. 새로운 기물을 폰의 '원래 있던 위치'에 생성
        string newPieceName = playerColor + "_" + pieceType;
        GameObject newPiece = Create(newPieceName, startX, startY);

        // 4. 생성된 새 기물이 애니메이션과 함께 '목표 위치'로 이동하도록 명령
        if (newPiece != null)
        {
            // ▼▼▼ 이 부분이 수정되었습니다! ▼▼▼
            // 이 프로모션이 공격이었는지 여부를 isCapture 변수에 저장합니다.
            bool isCapture = (capturedID != -1);

            // AnimateMove를 호출할 때 isCapture 값을 그대로 전달합니다.
            newPiece.GetComponent<PhotonView>().RPC("RPC_AnimateMove", RpcTarget.All, targetX, targetY, isCapture);
        }

        // 5. 프로모션이 끝났으니 상호작용을 다시 허용
        photonView.RPC("RPC_SetInteractionState", RpcTarget.All, false);

        // 6. 턴을 넘김
        CallNextTurn();
    }


    // 요청 2 처리: 플레이어의 선택을 받아 실제 프로모션을 실행하고 턴을 넘김
    [PunRPC]
    public void RequestPromotion(int pawnViewID, string pieceType)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject pawnObj = PhotonView.Find(pawnViewID)?.gameObject;
        if (pawnObj == null) return;

        MultiChessMan pawnCm = pawnObj.GetComponent<MultiChessMan>();
        int x = pawnCm.GetXBoard();
        int y = pawnCm.GetYBoard();
        string playerColor = pawnCm.GetPlayer();

        // 1. 기존 폰을 보드 배열에서 제거하고 네트워크에서 파괴
        SetPositionEmpty(x, y);
        PhotonNetwork.Destroy(pawnObj);

        // 2. 새 기물 생성
        string newPieceName = playerColor + "_" + pieceType;
        Create(newPieceName, x, y); // 기존의 Create 함수를 재활용

        // ▼▼▼ 프로모션이 끝났으니, 다시 상호작용을 허용하도록 RPC 호출 ▼▼▼
        photonView.RPC("RPC_SetInteractionState", RpcTarget.All, false);

        // 3. 모든 작업이 끝났으므로 턴을 넘김
        CallNextTurn();
    }

    [PunRPC]
    public void RPC_SetInteractionState(bool isBlocked)
    {
        this.isInteractionBlocked = isBlocked;
    }

    // MultiGame.cs 의 RequestEnterBastion 함수를 이 코드로 교체
    [PunRPC]
    public void RequestEnterBastion(int movingPieceID, int bastionID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView movingPieceView = PhotonView.Find(movingPieceID);
        PhotonView bastionView = PhotonView.Find(bastionID);

        if (movingPieceView == null || bastionView == null) return;

        // 1. 바스티온의 위치 정보를 가져옴
        int targetX = bastionView.GetComponent<MultiChessMan>().GetXBoard();
        int targetY = bastionView.GetComponent<MultiChessMan>().GetYBoard();

        // 2. 바스티온을 보이지 않게 하고, 주둔한 기물 ID를 기억하라고 명령
        bastionView.RPC("RPC_SetVisible", RpcTarget.All, false);
        // (필요하다면, 바스티온의 isOccupied 같은 상태도 여기서 RPC로 업데이트 가능)

        // 3. 진입한 기물에게 '주둔 상태'가 되었음을 알림 (어떤 바스티온에 들어갔는지 ID 포함)
        movingPieceView.RPC("RPC_SetGarrisonStatus", RpcTarget.All, true, bastionID);

        // 4. 진입한 기물을 애니메이션과 함께 바스티온의 위치로 이동시킴
        movingPieceView.RPC("RPC_AnimateMove", RpcTarget.All, targetX, targetY, false);

        // 5. 턴을 넘김
        CallNextTurn();
    }

    // MultiGame.cs 에 추가

    // 두 기물 이름을 받아 합성 결과를 반환하는 '레시피 북' 함수
    public string GetFusionResultType(string piece1Name, string piece2Name)
    {
        // 이름에서 색깔 부분("white_", "black_")을 제거해서 순수 기물 타입만 비교
        string type1 = piece1Name.Split('_')[1];
        string type2 = piece2Name.Split('_')[1];

        // 팔랑크스 (폰 + 폰)
        if ((type1 == "pawn" && type2 == "pawn")) return "PHALANX";

        // 테스투도 (폰 + 비숍)
        if ((type1 == "pawn" && type2 == "bishop") || (type1 == "bishop" && type2 == "pawn")) return "TESTUDO";

        // 슈발리에 (폰 + 나이트)
        if ((type1 == "pawn" && type2 == "knight") || (type1 == "knight" && type2 == "pawn")) return "CHEVALIER";

        // 바스티온 (폰 + 룩)
        if ((type1 == "pawn" && type2 == "rook") || (type1 == "rook" && type2 == "pawn")) return "BASTION";

        // 합성 불가능한 조합이면 null 반환
        return null;
    }

    // MultiGame.cs 에 추가
    [PunRPC]
    public void RequestFusion(int movingPieceID, int stationaryPieceID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject movingPiece = PhotonView.Find(movingPieceID)?.gameObject;
        GameObject stationaryPiece = PhotonView.Find(stationaryPieceID)?.gameObject;

        if (movingPiece == null || stationaryPiece == null) return;

        // 1. 합성될 기물들의 정보 가져오기
        MultiChessMan movingCm = movingPiece.GetComponent<MultiChessMan>();
        MultiChessMan stationaryCm = stationaryPiece.GetComponent<MultiChessMan>();
        int targetX = stationaryCm.GetXBoard();
        int targetY = stationaryCm.GetYBoard();
        string playerColor = movingCm.GetPlayer();

        // 2. 합성 결과물 이름 정하기
        string resultType = GetFusionResultType(movingPiece.name, stationaryPiece.name);
        string newPieceName = playerColor + "_" + resultType;

        // 3. 기존 두 기물 보드에서 제거 및 네트워크 파괴
        SetPositionEmpty(movingCm.GetXBoard(), movingCm.GetYBoard());
        SetPositionEmpty(stationaryCm.GetXBoard(), stationaryCm.GetYBoard());
        PhotonNetwork.Destroy(movingPiece);
        PhotonNetwork.Destroy(stationaryPiece);

        // 4. 새로운 합성 기물 생성
        Create(newPieceName, targetX, targetY);

        // 5. 턴 넘기기
        CallNextTurn();
    }


    // MultiGame.cs 에 새로 추가
    [PunRPC]
    public void RPC_PlacePieceOnBoard(int viewID)
    {
        PhotonView pieceView = PhotonView.Find(viewID);
        if (pieceView != null)
        {
            SetPosition(pieceView.gameObject);
        }
    }




    //Update() 유니티에서 제공하는 유니티 이벤트 메소드로 프레임이 재생될때마다 호출되는 메소드 입니다
    //gameOver가 true이고 마우스 좌클릭 상태일때 게임을 재시작합니다.
    public void Update()
    {
        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            gameOver = false;

            SceneManager.LoadScene("MultiTestGameScene");
        }
    }

    public void Winner(string playerWinner)
    {
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().text = playerWinner + " is the winner";
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().enabled = true;

        GameObject.FindGameObjectWithTag("RestartText").GetComponent<TextMeshProUGUI>().enabled = true;
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<TextMeshProUGUI>().text = "Tab To Restart";
        gameOver = true;

    }
}