using Photon.Pun;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.GraphicsBuffer;

public class MultiChessMan : MonoBehaviourPunCallbacks, IPunObservable 
{
    // References
    public GameObject controller;
    public GameObject movePlate;

    //폰이 안움직일 경우 두 칸 이동할 수 있도록 하는 부울 변수
    private bool pawnNeverMove = true;
    private bool kingNeverMove = true;
    // 0~7 체스판 좌표
    private int xBoard = -1;
    private int yBoard = -1;

    // Variable to keep track of "black" player of "white" player
    private string player;

    // Chessman.cs
    public string GetPlayer()
    {
        return player;
    }


    // References for all the sptrites that the chesspiece can be
    public GameObject black_queenP, black_knightP, black_bishopP, black_kingP, black_rookP, black_pawnP;
    public GameObject white_queenP, white_knightP, white_bishopP, white_kingP, white_rookP, white_pawnP;

    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    void Awake() 
    {
        black_queen = black_queenP.GetComponent<SpriteRenderer>().sprite;
        black_knight = black_knightP.GetComponent<SpriteRenderer>().sprite;
        black_bishop = black_bishopP.GetComponent<SpriteRenderer>().sprite;
        black_king = black_kingP.GetComponent<SpriteRenderer>().sprite;
        black_rook = black_rookP.GetComponent<SpriteRenderer>().sprite;
        black_pawn = black_pawnP.GetComponent<SpriteRenderer>().sprite;

        white_queen = white_queenP.GetComponent<SpriteRenderer>().sprite;
        white_knight = white_knightP.GetComponent<SpriteRenderer>().sprite;
        white_bishop = white_bishopP.GetComponent<SpriteRenderer>().sprite;
        white_king = white_kingP.GetComponent<SpriteRenderer>().sprite;
        white_rook = white_rookP.GetComponent<SpriteRenderer>().sprite;
        white_pawn = white_pawnP.GetComponent<SpriteRenderer>().sprite;

    }

    // 말 초기화: 게임 컨트롤러 찾고, 스프라이트 & 플레이어 세팅, 화면 위치 이동
    public void Activate()
    {

        controller = GameObject.FindGameObjectWithTag("GameController");


        // take the instantiated location and adjust the transform
        SetCoords();

        switch (this.name)
        {
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; player = "black"; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; player = "black"; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; player = "black"; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; player = "black"; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; player = "black"; break;

            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; player = "white"; break;
        }
    }

    public void SetCoords()
    {
        float x = xBoard;
        float y = yBoard;

        x *= 0.66f;
        y *= 0.66f;

        x += -2.3f;
        y += -2.3f;

        this.transform.position = new Vector3(x, y, -1.0f);
    }

    public void DisableDoubleMove()
    {
        pawnNeverMove = false;
    }

    public void DisableCastling()
    {
        kingNeverMove = false;
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        this.xBoard = x;
    }

    public void SetYBoard(int y)
    {
        this.yBoard = y;
    }

    [PunRPC]
    public void SetupSprite(string pieceName, int x, int y)
    {
        this.name = pieceName;
        SetXBoard(x);
        SetYBoard(y);
        Activate();

        // 모든 클라이언트에서 보드 배열에 등록
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<MultiGame>();
        game.SetPosition(this.gameObject);
    }

    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]);
        }
    }


    [PunRPC]
    public void MoveTo(int targetX, int targetY)
    {
        // 1) 원래 위치 빈칸 처리
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<MultiGame>();
        game.SetPositionEmpty(xBoard, yBoard);

        // 2) 좌표 갱신
        xBoard = targetX;
        yBoard = targetY;
        SetCoords();

        // 3) 보드 배열에 재등록
        game.SetPosition(this.gameObject);
    }


    [PunRPC]
    public void DestroySelf()
    {
        // 1) 모든 클라이언트에서 보드 배열 갱신
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<MultiGame>();
        game.SetPositionEmpty(xBoard, yBoard);

        // 2) 오너만 파괴
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }


    public void InitiateMovePlates()
    {
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(1, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                LineMovePlate(-1, -1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                break;
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1, 1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, -1);
                break;
            case "black_king":
            case "white_king":
                SurroundMovePlate();
                CastleingMovePlate();
                break;
            case "black_rook":
            case "white_rook":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                break;
            case "black_pawn":
                PawnMovePlate(xBoard, yBoard - 1);
                break;
            case "white_pawn":
                PawnMovePlate(xBoard, yBoard + 1);
                break;
        }
    }

    // *** “LineMovePlate” : xIncrement, yIncrement 방향으로
    //    빈 칸이 나올 때까지 계속 생성 → 상대 말 만나면 “공격” 표시
    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        MultiGame sc = controller.GetComponent<MultiGame>();

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<MultiChessMan>().player != player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }

    // *** “LMovePlate” : 나이트 이동 
    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }
    // *** “SurroundMovePlate” : 주변 8칸 - 킹 이동
    public void SurroundMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard);
        PointMovePlate(xBoard + 1, yBoard);
        PointMovePlate(xBoard + 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard - 1, yBoard - 1);
    }
    public void CastleingMovePlate()
    {
        MultiGame sc = controller.GetComponent<MultiGame>();

        if (!kingNeverMove) return;

        // 왼쪽 캐슬링
        if (sc.checkLeftSide(xBoard, yBoard)) CastlingPlateSpawn(xBoard - 2, yBoard);

        // 오른쪽 캐슬링
        if (sc.checkRightSide(xBoard, yBoard)) CastlingPlateSpawn(xBoard + 2, yBoard);

    }


    private void CastlingPlateSpawn(int targetX, int targetY)
    {
        // 화면 좌표 변환
        float x = targetX * 0.66f - 2.3f;
        float y = targetY * 0.66f - 2.3f;

        // MovePlate 인스턴스화
        object[] initData = new object[] { targetX, targetY };
        GameObject mp = PhotonNetwork.Instantiate("MultiMovePlate", new Vector3(x, y, -3.0f), Quaternion.identity, 0, initData);

        var mpScript = mp.GetComponent<MultiMovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(targetX, targetY);

        // 여기가 핵심: 이 플레이트가 캐슬링용임을 표시
        mpScript.isCastling = true;
    }


    // *** “PointMovePlate” : 단일 좌표 체크 → 빈 칸이면 MovePlate, 적이면 공격 MovePlate
    public void PointMovePlate(int x, int y)
    {
        MultiGame sc = controller.GetComponent<MultiGame>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }
            else if (cp.GetComponent<MultiChessMan>().player != player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }
    }

    public void PawnMovePlate(int x, int y)
    {
        MultiGame sc = controller.GetComponent<MultiGame>();

        int direction = (player == "white") ? 1 : -1;

        // 1. 한 칸 전진
        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);

            // 2. 두 칸 전진 (처음 움직이는 경우에만)
            if (pawnNeverMove)
            {
                int twoStepY = y + direction;
                if (sc.PositionOnBoard(x, twoStepY) && sc.GetPosition(x, twoStepY) == null)
                {
                    MovePlateSpawn(x, twoStepY);
                }
            }
        }

        // 3. 대각선 공격
        for (int dx = -1; dx <= 1; dx += 2)
        {
            int diagX = x + dx;
            int diagY = y;  
            if (sc.PositionOnBoard(diagX, diagY)
                && sc.GetPosition(diagX, diagY) != null
                && sc.GetPosition(diagX, diagY).GetComponent<MultiChessMan>().player != player)
            {
                MovePlateAttackSpawn(diagX, diagY);
            }

        }
    }


    // 실제로 MovePlate 프리팹을 Instantiate 하는 두 메소드
    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        float x = matrixX;
        float y = matrixY;

        x *= 0.66f;
        y *= 0.66f;

        x += -2.3f;
        y += -2.3f;

        object[] initData = new object[] { matrixX, matrixY };
        GameObject mp = PhotonNetwork.Instantiate("MultiMovePlate", new Vector3(x, y, -3.0f), Quaternion.identity, 0, initData);

        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        float x = matrixX;
        float y = matrixY;

        x *= 0.66f;
        y *= 0.66f;

        x += -2.3f;
        y += -2.3f;

        object[] initData = new object[] { matrixX, matrixY };
        GameObject mp = PhotonNetwork.Instantiate("MultiMovePlate", new Vector3(x, y, -3.0f), Quaternion.identity, 0, initData);
        //GameObject mp = PhotonNetwork.Instantiate("MultiMovePlate", new Vector3(x, y, -3.0f), Quaternion.identity);

        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    private void OnMouseUp()
    {
        var game = controller.GetComponent<MultiGame>();

        if (!controller.GetComponent<MultiGame>().IsGameOver() && controller.GetComponent<MultiGame>().GetCurrentPlayer() == player 
            && controller.GetComponent<MultiGame>().GetCurrentPlayer() == controller.GetComponent<MultiGame>().GetMyPlayerColor() )
        {
            Debug.Log("===== 보드 상태 (이동 전) =====");
            game.DebugPrintBoard();

            DestroyMovePlates();

            InitiateMovePlates();

            // 디버그: 이동 후에도 찍어 보고 싶다면
            Debug.Log("===== 보드 상태 (이동 후) =====");
            game.DebugPrintBoard();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 소유자가 데이터를 보냄
            stream.SendNext(xBoard);
            stream.SendNext(yBoard);
            stream.SendNext(transform.position);
        }
        else
        {
            // 다른 클라이언트가 데이터 수신
            xBoard = (int)stream.ReceiveNext();
            yBoard = (int)stream.ReceiveNext();
            Vector3 pos = (Vector3)stream.ReceiveNext();

            // 좌표와 위치 갱신
            transform.position = pos;
        }
    }
}