using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class MultiGame : MonoBehaviourPunCallbacks
{
    public GameObject chesspiece;
    public Sprite boardSprite;
    public Sprite backgroundSprite;

    // Positions and team for each chesspiece
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private string myPlayerColor;
    private string currentPlayer ;

    private bool gameOver = false;
    private PhotonView pv;

    public float tileSize = 0.66f;                 // 자동 재계산 되지만 기본값
    public Vector2 boardOrigin = Vector2.zero;     // 0,0 칸(=a1)의 월드 좌표(센터)
    public float boardWorldWidth = 0f;
    public float boardWorldHeight = 0f;



    void Awake()
    {

   
        myPlayerColor = PlayerColorManager.instance.GetMyToggle() ? "white" : "black";
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

    public GameObject Create(string name, int x, int y)
    {
        Vector3 spawnPos = new Vector3(0f, 0f, 0);
        GameObject obj = PhotonNetwork.Instantiate("Pieces/MultiChesspiece", spawnPos, Quaternion.identity);
        var sr = obj.GetComponent<SpriteRenderer>();

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

    [PunRPC]
    public void RequestMovePiece(int attackerID, int targetX, int targetY, int capturedID, bool isCastle)
    {
        
        // 방장이 아니면 아무것도 하지 않음 (보안)
        if (!PhotonNetwork.IsMasterClient) return;

        // --- 여기에서 방장은 전달받은 정보로 이동이 유효한지 검증할 수 있습니다 ---
        // 예: 턴이 맞는지, 규칙에 맞는지 등 (지금은 생략)
        // ---------------------------------------------------------------------

        // 캐슬링 요청일 경우
        if (isCastle)
        {
            // 캐슬링 실행 RPC를 모든 클라이언트에게 보냄
            photonView.RPC("RPC_ExecuteCastling", RpcTarget.All, attackerID, targetX);
        }
        else // 일반 이동/공격 요청일 경우
        {
            // 1. 잡히는 말이 있으면 파괴 RPC 실행
            if (capturedID != -1)
            {
                PhotonView.Find(capturedID)?.RPC("DestroySelf", RpcTarget.AllBuffered);
            }

            // 2. 공격하는 말을 이동시키는 RPC 실행
            PhotonView.Find(attackerID)?.RPC("MoveTo", RpcTarget.AllBuffered, targetX, targetY);
        }

        // ▼▼▼ 말의 첫 이동 상태를 업데이트하는 RPC 호출 ▼▼▼
        PhotonView.Find(attackerID)?.RPC("RPC_UpdateMovedStatus", RpcTarget.All);

        // 4. 모든 처리가 끝났으므로, 다음 턴으로 넘김
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
        kingCm.SetXBoard(kingTargetX); // 1. 내부 X좌표를 먼저 바꾸고
                                       // kingCm.SetYBoard(y); // y는 그대로지만, 명확성을 위해 써줘도 좋습니다.
        kingCm.SetCoords();           // 2. 바뀐 내부 좌표를 보고 실제 위치를 옮기게 한다.

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
            rookCm.SetXBoard(rookTargetX); // 1. 내부 X좌표를 먼저 바꾸고
            rookCm.SetCoords();            // 2. 실제 위치를 옮기게 한다.

            SetPosition(rookObj);
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