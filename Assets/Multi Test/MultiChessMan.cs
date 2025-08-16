using Photon.Pun;
using UnityEngine;
using System.Collections;

public class MultiChessMan : MonoBehaviourPunCallbacks, IPunObservable 
{


    // References
    public GameObject controller;
    private MultiGame gameController; // 스크립트 참조를 저장할 변수
    public GameObject movePlate;

    //폰이 안움직일 경우 두 칸 이동할 수 있도록 하는 부울 변수
    private bool pawnNeverMove = true;
    private bool kingNeverMove = true;
    private bool rookNeverMove = true; // 룩을 위한 변수 추가


    // 0~7 체스판 좌표
    private int xBoard = -1;
    private int yBoard = -1;

    // Variable to keep track of "black" player of "white" player
    private string player;


    [Header("Animation Settings")]
    public float moveDuration = 0.3f; // 말이 이동하는 데 걸리는 시간 (초)
    public float hopHeight = 0.5f;    // 나이트가 점프하는 높이

    // Chessman.cs
    public string GetPlayer()
    {
        return player;
    }

    // 이 킹이 움직인 적이 없는지 외부에서 확인할 수 있게 해주는 함수
    public bool GetKingNeverMove()
    {
        return this.kingNeverMove;
    }

    // 이 룩이 움직인 적이 없는지 외부에서 확인할 수 있게 해주는 함수
    public bool GetRookNeverMove()
    {
        return this.rookNeverMove;
    }

    // MultiChessMan.cs 에 추가
    [PunRPC]
    public void RPC_UpdateMovedStatus()
    {
        // 이제 이 말은 처음 움직인 것이 아니게 됨
        this.pawnNeverMove = false;
        this.kingNeverMove = false;
        this.rookNeverMove = false;
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

        controller = GameObject.FindGameObjectWithTag("GameController");
        if (controller != null)
        {
            gameController = controller.GetComponent<MultiGame>();
        }
        else
        {
            // 만약 못찾으면 에러 메시지를 확실하게 남겨서 디버깅을 돕습니다.
            Debug.LogError("!!!!!!!! GameController 오브젝트를 찾을 수 없습니다! Tag가 정확한지 확인하세요. !!!!!!!!");
        }

        // ... 기존의 다른 Awake() 코드 (스프라이트 로드 등)가 있었다면 그대로 둡니다 ...

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
        // 이전에 Awake()에서 찾아둔 gameController 변수를 사용합니다.
        if (gameController == null)
        {
            Debug.LogError($"SetCoords Error: gameController is not assigned on {this.name}!");
            return;
        }

        float t = gameController.tileSize;
        Vector2 origin = gameController.boardOrigin;

        float worldX = origin.x + xBoard * t;
        float worldY = origin.y + yBoard * t;

        // z 값은 -1로 고정하여 안정성을 높입니다.
        transform.position = new Vector3(worldX, worldY, -1.0f);
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

        // ▼▼▼ 렌더링 순서 설정 코드 추가 ▼▼▼
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Pieces"; // "Pieces" 층에 그리도록 설정
            sr.sortingOrder = 1; // 그냥 1로 두면 됩니다. sorting layer를 썼기 때문에 이미 pieces는 2층에 안착. 2층의 order 1이다.
        }


        // === 스케일 자동 조정 ===
        if (sr != null && sr.sprite != null)
        {
            // sprite 원본 폭 (월드 단위) - bounds는 transform.localScale을 이미 반영함
            float spriteWidth = sr.sprite.bounds.size.x;

            // 가져온 tileSize 사용
            float tile = game.tileSize-0.1f;

            // 칸 크기에 대해 sprite가 차지할 비율(예: 0.8 => 칸의 80% 너비로 맞춤)
            float fillRatio = 0.65f;

            // 원하는 실제 폭 = tile * fillRatio
            float desiredWorldWidth = tile * fillRatio;

            // 현재 sprite.bounds.size.x 는 '원본 1.0 scale'에서의 폭이지만, 
            // sr.sprite.bounds.size.x * transform.localScale.x = 현재 world 폭.
            // 우리는 transform.localScale을 다시 설정할 것이므로 아래처럼 계산:
            float originalSpriteWidth = sr.sprite.bounds.size.x; // units at scale=1

            if (originalSpriteWidth > 0.0001f)
            {
                float scale = desiredWorldWidth / originalSpriteWidth;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        // 최종적으로 보드 배열에 등록 및 coords 세팅
        SetCoords();
        var gameRef = GameObject.FindGameObjectWithTag("GameController").GetComponent<MultiGame>();
        gameRef.SetPosition(this.gameObject);

        // 정렬 레이어 지정 (안정성)
        if (sr != null)
        {
            sr.sortingLayerName = "Pieces";
            sr.sortingOrder = 2;
        }
    }

    // 2. MovePlate 파괴 함수 수정
    public void DestroyMovePlates()
    {
        // 태그로 찾아서 간단하게 파괴
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]);
        }
    }

    // MultiChessMan.cs

    public void ExecuteMove(int targetX, int targetY, bool isAttack, bool isCastle)
    {
        // 1. Awake에서 캐싱해둔 gameController를 사용 (안전하고 효율적)
        if (gameController == null) return;

        // 2. 프로모션 조건인지 정확하게 확인 (white는 7번, black은 0번 행)
        bool isPromotion = (this.name.Contains("pawn") &&
                           ((player == "white" && targetY == 2) || (player == "black" && targetY == 5)));

        // 3. 프로모션이 '아닌' 모든 일반 이동/캐슬링의 경우
        if (!isPromotion)
        {
            // 잡히는 말이 있는지 확인
            int capturedID = -1;
            if (isAttack)
            {
                GameObject capturedPiece = gameController.GetPosition(targetX, targetY);
                if (capturedPiece != null) capturedID = capturedPiece.GetComponent<PhotonView>().ViewID;
            }

            // 방장에게 '일반 이동'을 요청하는 RPC를 보냄
            gameController.GetComponent<PhotonView>().RPC("RequestMovePiece", RpcTarget.MasterClient,
                photonView.ViewID, targetX, targetY, capturedID, isCastle);
        }
        // 4. 프로모션인 '특별한' 경우
        else
        {
            // 잡히는 말이 있는지 확인 (프로모션과 동시에 잡는 경우)
            int capturedID = -1;
            if (isAttack)
            {
                GameObject capturedPiece = gameController.GetPosition(targetX, targetY);
                if (capturedPiece != null) capturedID = capturedPiece.GetComponent<PhotonView>().ViewID;
            }

            // 방장에게 RPC를 보내는 대신, 로컬의 PromotionManager UI를 직접 띄운다.
            PromotionManager.Instance.ShowPromotionUI(this, targetX, targetY, isAttack, capturedID);
        }

        // 5. 어떤 경우든, 이동을 시작했으니 MovePlate는 모두 제거
        DestroyMovePlates();
    }



    [PunRPC]
    public void RPC_AnimateMove(int targetX, int targetY)
    {
        // 1. 게임의 논리적 상태를 먼저 즉시 업데이트 (매우 중요!)
        gameController.SetPositionEmpty(xBoard, yBoard); // 원래 있던 칸 비우기
        xBoard = targetX;
        yBoard = targetY;
        gameController.SetPosition(this.gameObject); // 새 칸에 등록하기

        // 2. 목표 월드 좌표 계산
        Vector3 targetPosition = new Vector3(
            gameController.boardOrigin.x + xBoard * gameController.tileSize,
            gameController.boardOrigin.y + yBoard * gameController.tileSize,
            -1.0f // z 위치 고정
        );

        // 3. 기물 종류에 따라 다른 애니메이션 코루틴을 실행
        if (this.name.Contains("knight"))
        {
            StartCoroutine(AnimateHop(targetPosition));
        }
        else
        {
            StartCoroutine(AnimateSlide(targetPosition));
        }
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
        // Awake에서 캐싱해둔 gameController를 사용하는 것이 더 안정적입니다.
        if (gameController == null) return;

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        // 1. 경로가 비어있는 동안에는 일반 MovePlate를 계속 생성합니다.
        while (gameController.PositionOnBoard(x, y) && gameController.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        // 2. 루프가 끝난 지점(장애물을 만난 지점)을 다시 한번 확인합니다.
        if (gameController.PositionOnBoard(x, y))
        {
            // 3. 그곳에 있는 기물을 가져옵니다.
            GameObject targetPiece = gameController.GetPosition(x, y);

            // 4. 기물이 존재하고, 그 기물이 내 편이 아닐 경우에만 공격 MovePlate를 생성합니다.
            if (targetPiece != null && targetPiece.GetComponent<MultiChessMan>().player != this.player)
            {
                MovePlateAttackSpawn(x, y);
            }
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
    // MultiChessMan.cs 의 CastleingMovePlate 함수
    public void CastleingMovePlate()
    {
        if (!kingNeverMove) return;

        // gameController는 Awake에서 캐싱해 뒀다고 가정
        if (gameController.CanCastle(xBoard, yBoard, true)) // 킹 사이드(오른쪽) 체크
        {
            CastlingPlateSpawn(xBoard + 2, yBoard);
        }
        if (gameController.CanCastle(xBoard, yBoard, false)) // 퀸 사이드(왼쪽) 체크
        {
            CastlingPlateSpawn(xBoard - 2, yBoard);
        }
    }


    // MultiChessMan.cs

    private void CastlingPlateSpawn(int targetX, int targetY)
    {
        // gameController는 Awake()에서 캐싱해두었다고 가정합니다.
        if (gameController == null)
        {
            Debug.LogError("Game Controller가 없어서 CastlingPlate를 생성할 수 없습니다.");
            return;
        }

        // 1. 하드코딩된 좌표 계산을 모두 삭제하고, 기본값으로 생성합니다.
        GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity);

        // 2. 생성된 객체의 스크립트를 가져옵니다.
        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();

        // 3. 참조와 논리적 좌표를 설정합니다.
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(targetX, targetY);

        // 4. ★★★ 이 플레이트가 캐슬링용임을 표시합니다. ★★★
        mpScript.isCastling = true;

        // 5. 이제 모든 시각적 처리는 SetupVisuals 함수에게 맡깁니다.
        mpScript.SetupVisuals(gameController, targetX, targetY);
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
        if (gameController == null)
        {
            // 만약을 대비한 방어 코드
            Debug.LogError("Game Controller가 할당되지 않아 MovePlate를 생성할 수 없습니다.");
            return;
        }

        // 1. 프리팹을 기본값으로 생성
        GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity);

        // 2. 생성된 객체의 스크립트를 가져옴
        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();

        // 3. 참조와 좌표 설정
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);

        // 4. ★★★ 새로 만든 시각화 함수 호출 ★★★
        // 게임 컨트롤러와 보드 좌표를 넘겨주면 알아서 위치/크기를 설정함
        mpScript.SetupVisuals(gameController, matrixX, matrixY);
    }


    // MultiChessMan.cs

    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        if (gameController == null) return;

        // 1. 프리팹 생성
        GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity);

        // 2. 스크립트 가져오기
        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();

        // 3. ★★★ attack 플래그 설정 (가장 중요!) ★★★
        mpScript.attack = true;

        // 4. 나머지 설정 및 시각화 함수 호출
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
        mpScript.SetupVisuals(gameController, matrixX, matrixY);
    }

    private void OnMouseUp()
    {
        var game = controller.GetComponent<MultiGame>();


        // 이전에 Awake()에서 캐싱해둔 gameController 변수를 사용해야 합니다.
        if (gameController == null) return;

        // ▼▼▼ !gameController.isInteractionBlocked 조건을 이 if문에 추가합니다 ▼▼▼
        if (!gameController.IsGameOver() && !gameController.isInteractionBlocked && gameController.GetCurrentPlayer() == player
            && gameController.GetCurrentPlayer() == gameController.GetMyPlayerColor())
        {
            DestroyMovePlates();

            InitiateMovePlates(); 
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

    private IEnumerator AnimateSlide(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            // 시간에 따라 시작 위치와 목표 위치 사이를 부드럽게 보간(Lerp)
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 애니메이션이 끝난 후 정확한 위치에 고정
        transform.position = targetPosition;
    }

    // 2. 나이트 이동 애니메이션 (홉/점프)
    private IEnumerator AnimateHop(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            // x, z축은 선형으로 움직이되, y축(높이)은 위아래로 움직여 점프 효과를 줌
            float progress = elapsedTime / moveDuration;
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            currentPos.y += hopHeight * Mathf.Sin(progress * Mathf.PI); // Sin 함수로 부드러운 아크 생성

            transform.position = currentPos;
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 애니메이션이 끝난 후 정확한 위치에 고정
        transform.position = targetPosition;
    }
}