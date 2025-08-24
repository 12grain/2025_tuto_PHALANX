using Photon.Pun;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

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
    private bool isGarrisoned = false;
    private bool hasShield = false;
    private bool phalanxShieldActive = true;
    private int garrisonedBastionID = -1;


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

    // MultiChessMan.cs 에 추가
    public bool HasShield()
    {
        return this.hasShield;
    }

    // MultiGame.cs 에서는 capturedCm.hasShield 대신 capturedCm.HasShield() 를 사용

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

    // MultiChessMan.cs 클래스 내부에 추가
    // 이 기물이 주둔 상태인지 외부에서 확인하기 위한 함수
    public bool IsGarrisoned()
    {
        return isGarrisoned;
    }

    // 어떤 바스티온에 주둔했는지 외부에서 확인하기 위한 함수
    public int GetGarrisonedBastionID()
    {
        return garrisonedBastionID;
    }

    [PunRPC]
    public void RPC_SetGarrisonStatus(bool status, int bastionID)
    {
        this.isGarrisoned = status;
        this.garrisonedBastionID = bastionID;
    }

    // 바스티온 전용: 자신을 숨기거나 나타나게 하는 RPC
    [PunRPC]
    public void RPC_SetVisible(bool isVisible)
    {
        this.gameObject.SetActive(isVisible);
    }

    // References for all the sptrites that the chesspiece can be
    public GameObject black_queenP, black_knightP, black_bishopP, black_kingP, black_rookP, black_pawnP;
    public GameObject white_queenP, white_knightP, white_bishopP, white_kingP, white_rookP, white_pawnP;
    public GameObject white_PHALANX_P, white_TESTUDO_P, white_CHEVALIER_P, white_BASTION_P;
    public GameObject black_PHALANX_P, black_TESTUDO_P, black_CHEVALIER_P, black_BASTION_P;

    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;
    public Sprite white_PHALANX, white_TESTUDO, white_CHEVALIER, white_BASTION;
    public Sprite black_PHALANX, black_TESTUDO, black_CHEVALIER, black_BASTION;

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

        // ▼▼▼ 여기에 새로운 스프라이트 로드 코드 추가 ▼▼▼
        white_PHALANX = white_PHALANX_P.GetComponent<SpriteRenderer>().sprite;
        white_TESTUDO = white_TESTUDO_P.GetComponent<SpriteRenderer>().sprite;
        white_CHEVALIER = white_CHEVALIER_P.GetComponent<SpriteRenderer>().sprite;
        white_BASTION = white_BASTION_P.GetComponent<SpriteRenderer>().sprite;

        black_PHALANX = black_PHALANX_P.GetComponent<SpriteRenderer>().sprite;
        black_TESTUDO = black_TESTUDO_P.GetComponent<SpriteRenderer>().sprite;
        black_CHEVALIER = black_CHEVALIER_P.GetComponent<SpriteRenderer>().sprite;
        black_BASTION = black_BASTION_P.GetComponent<SpriteRenderer>().sprite;

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
            case "black_PHALANX": this.GetComponent<SpriteRenderer>().sprite = black_PHALANX; player = "black"; break;
            case "black_TESTUDO": this.GetComponent<SpriteRenderer>().sprite = black_TESTUDO; player = "black"; break;
            case "black_CHEVALIER": this.GetComponent<SpriteRenderer>().sprite = black_CHEVALIER; player = "black"; break;
            case "black_BASTION": this.GetComponent<SpriteRenderer>().sprite = black_BASTION; player = "black"; break;

            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; player = "white"; break;
            case "white_PHALANX": this.GetComponent<SpriteRenderer>().sprite = white_PHALANX; player = "white"; break;
            case "white_TESTUDO": this.GetComponent<SpriteRenderer>().sprite = white_TESTUDO; player = "white"; break;
            case "white_CHEVALIER": this.GetComponent<SpriteRenderer>().sprite = white_CHEVALIER; player = "white"; break;
            case "white_BASTION": this.GetComponent<SpriteRenderer>().sprite = white_BASTION; player = "white"; break;

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

    public void ExecuteMove(int targetX, int targetY, bool isAttack, bool isCastle, bool isSpecialMove)
    {
        if (gameController == null) return;

        // 특수 이동(합성 또는 바스티온 진입) 처리
        if (isSpecialMove)
        {
            GameObject targetPiece = gameController.GetPosition(targetX, targetY);
            if (targetPiece != null)
            {
                int targetID = targetPiece.GetComponent<PhotonView>().ViewID;
                if (targetPiece.name.Contains("BASTION")) // 목표가 바스티온이면
                {
                    gameController.GetComponent<PhotonView>().RPC("RequestEnterBastion", RpcTarget.MasterClient, photonView.ViewID, targetID);
                }
                else // 그 외는 합성
                {
                    gameController.GetComponent<PhotonView>().RPC("RPC_StartFusionAnimation", RpcTarget.All, photonView.ViewID, targetID);
                }
            }
        }
        // ▼▼▼ "만약 합성이 아니라면" 이라는 'else if'로 묶어줍니다 ▼▼▼
        else
        {
            // 2. 프로모션 조건인지 정확하게 확인
            bool isPromotion = (this.name.Contains("pawn") &&
                               ((player == "white" && targetY == 7) || (player == "black" && targetY == 0)));

            if (isPromotion)
            {
                // 3. 프로모션인 경우
                int capturedID = -1;
                if (isAttack)
                {
                    GameObject capturedPiece = gameController.GetPosition(targetX, targetY);
                    if (capturedPiece != null) capturedID = capturedPiece.GetComponent<PhotonView>().ViewID;
                }
                PromotionManager.Instance.ShowPromotionUI(this, targetX, targetY, isAttack, capturedID);
            }
            else
            {
                // 4. 그 외 모든 일반 이동/캐슬링인 경우
                int capturedID = -1;
                if (isAttack)
                {
                    GameObject capturedPiece = gameController.GetPosition(targetX, targetY);
                    if (capturedPiece != null) capturedID = capturedPiece.GetComponent<PhotonView>().ViewID;
                }
                gameController.GetComponent<PhotonView>().RPC("RequestMovePiece", RpcTarget.MasterClient,
                    photonView.ViewID, targetX, targetY, capturedID, isCastle);
            }
        }

        DestroyMovePlates();
    }



    // MultiChessMan.cs

    [PunRPC]
    public void RPC_AnimateMove(int targetX, int targetY, bool isCapture)
    {
        if (!isCapture)
        {
            SoundManager.Instance.PlayMoveSound();
        }

        // ▼▼▼ "SetPositionEmpty" 코드를 다시 추가합니다! ▼▼▼
        if (gameController != null)
        {
            // 모든 클라이언트가 자신의 지도에서 나의 옛날 자리를 비운다.
            gameController.SetPositionEmpty(this.xBoard, this.yBoard);
        }

        // 1. 자신의 '새로운' 논리적 위치를 업데이트
        xBoard = targetX;
        yBoard = targetY;
        if (gameController != null)
        {
            gameController.SetPosition(this.gameObject); // 새 칸에 자신을 등록
        }

        // 2. 목표 월드 좌표 계산 및 애니메이션 실행 (이 부분은 동일)
        Vector3 targetPosition = new Vector3(
            gameController.boardOrigin.x + xBoard * gameController.tileSize,
            gameController.boardOrigin.y + yBoard * gameController.tileSize,
            -1.0f
        );

        if (this.name.Contains("knight"))
        {
            StartCoroutine(AnimateHop(targetPosition));
        }
        else
        {
            StartCoroutine(AnimateSlide(targetPosition));
        }
    }

    // 합성 애니메이션 코루틴
    private IEnumerator AnimateFusion(GameObject otherPiece)
    {
        // 애니메이션이 진행되는 동안은 다른 입력을 막음
        if (gameController != null)
        {
            gameController.isInteractionBlocked = true;
        }

        Vector3 startPosThis = this.transform.position;
        Vector3 startPosOther = otherPiece.transform.position;
        Vector3 targetPos = otherPiece.transform.position; // 재료가 되는 기물 위치로 모임

        float elapsedTime = 0f;
        float fusionAnimDuration = 0.5f; // 합성 애니메이션 시간

        while (elapsedTime < fusionAnimDuration)
        {
            // 두 기물이 목표 위치로 점점 다가감
            this.transform.position = Vector3.Lerp(startPosThis, targetPos, elapsedTime / fusionAnimDuration);
            otherPiece.transform.position = Vector3.Lerp(startPosOther, targetPos, elapsedTime / fusionAnimDuration);

            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 애니메이션이 끝난 후, 요청을 보냈던 클라이언트만 실제 합성을 요청함
        if (photonView.IsMine)
        {
            int stationaryPieceID = otherPiece.GetComponent<PhotonView>().ViewID;
            gameController.GetComponent<PhotonView>().RPC("RequestFusion", RpcTarget.MasterClient, photonView.ViewID, stationaryPieceID);
        }
    }

    // 애니메이션을 시작시키는 공개 함수 (RPC로 호출될 예정)
    public void StartFusionAnimationWith(GameObject otherPiece)
    {
        StartCoroutine(AnimateFusion(otherPiece));
    }


    [PunRPC]
    public void DestroySelf()
    {
        SoundManager.Instance.PlayKillSound();

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
        // ▼▼▼ 함수 맨 위에 인접 합성 체크 로직을 추가합니다 ▼▼▼
        CheckAdjacentFusions();


        // ▼▼▼ 함수 맨 위에 바스티온 버프 확인 로직을 추가합니다 ▼▼▼
        if (this.isGarrisoned)
        {
            // 만약 주둔 상태라면, 다른 모든 움직임은 무시하고 오직 룩처럼만 움직인다.
            Debug.Log(this.name + "가 바스티온에 주둔 중: 룩처럼 움직입니다!");
            LineMovePlate(1, 0);
            LineMovePlate(-1, 0);
            LineMovePlate(0, 1);
            LineMovePlate(0, -1);
            return; // 여기서 함수를 끝내서, 아래의 switch문이 실행되지 않도록 함
        }
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
            // ▼▼▼ 새로운 기물들의 case 추가 ▼▼▼
            case "white_PHALANX":
            case "black_PHALANX":
                PhalanxMovePlate();
                break;
            case "white_BASTION":
            case "black_BASTION":
                break;
            case "white_TESTUDO":
            case "black_TESTUDO":
                SurroundMovePlate();
                break;
            case "black_CHEVALIER":
            case "white_CHEVALIER":
                ChevalierMovePlate();
                break;


        }
    }

    // *** “LineMovePlate” : xIncrement, yIncrement 방향으로
    //    빈 칸이 나올 때까지 계속 생성 → 상대 말 만나면 “공격” 표시
    // MultiChessMan.cs

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        if (gameController == null) return;

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (gameController.PositionOnBoard(x, y) && gameController.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (gameController.PositionOnBoard(x, y))
        {
            GameObject targetPiece = gameController.GetPosition(x, y);
            if (targetPiece != null)
            {
                MultiChessMan targetCm = targetPiece.GetComponent<MultiChessMan>();
                if (targetCm.player != this.player) // 적군이면 -> 공격
                {
                    MovePlateAttackSpawn(x, y);
                }
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


    // MultiChessMan.cs 에 추가

    public void PhalanxMovePlate()
    {
        if (gameController == null) return;

        // 플레이어 색상에 따라 '앞' 방향을 결정
        int forwardDirection = (player == "white") ? 1 : -1;

        // --- 규칙 1: 전방 1칸 이동 ---
        int moveX = xBoard;
        int moveY = yBoard + forwardDirection;

        // 이동할 칸이 보드 안이고 비어있을 때만 이동 가능
        if (gameController.PositionOnBoard(moveX, moveY) && gameController.GetPosition(moveX, moveY) == null)
        {
            MovePlateSpawn(moveX, moveY);
        }

        // --- 규칙 2: 3방향 공격 (전방, 좌, 우) ---
        // 공격할 좌표들을 담을 배열
        int[] attackXCoords = { xBoard, xBoard - 1, xBoard + 1 };
        int[] attackYCoords = { yBoard + forwardDirection, yBoard, yBoard };

        // 각 공격 방향을 순서대로 확인
        for (int i = 0; i < 3; i++)
        {
            int currentAttackX = attackXCoords[i];
            int currentAttackY = attackYCoords[i];

            // 공격할 칸이 보드 안일 때만 확인
            if (gameController.PositionOnBoard(currentAttackX, currentAttackY))
            {
                GameObject pieceToAttack = gameController.GetPosition(currentAttackX, currentAttackY);
                // 그곳에 기물이 있고, 그 기물이 '적'일 경우 공격 가능
                if (pieceToAttack != null && pieceToAttack.GetComponent<MultiChessMan>().player != this.player)
                {
                    MovePlateAttackSpawn(currentAttackX, currentAttackY);
                }
            }
        }
    }

    // MultiChessMan.cs

    // MultiChessMan.cs

    public void ChevalierMovePlate()
    {
        if (gameController == null) return;

        // 4개의 직선 방향 (상, 하, 좌, 우)
        int[] xDirections = { 0, 0, 1, -1 };
        int[] yDirections = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            // --- 일반 이동 (1칸 & 2칸, 경로 확인 필요) ---
            int x1 = xBoard + xDirections[i];
            int y1 = yBoard + yDirections[i];

            if (gameController.PositionOnBoard(x1, y1))
            {
                // 1칸 앞을 PointMovePlate로 확인 (비었는지, 적인지, 아군인지 알아서 판단)
                PointMovePlate(x1, y1);

                // 2칸 앞은 1칸 앞이 비어있을 때만 갈 수 있음
                if (gameController.GetPosition(x1, y1) == null)
                {
                    int x2 = xBoard + xDirections[i] * 2;
                    int y2 = yBoard + yDirections[i] * 2;
                    if (gameController.PositionOnBoard(x2, y2))
                    {
                        // 2칸 앞도 PointMovePlate로 확인
                        PointMovePlate(x2, y2);
                    }
                }
            }

            // --- 돌진 공격 (3칸, 중간 장애물 무시) ---
            int x3 = xBoard + xDirections[i] * 3;
            int y3 = yBoard + yDirections[i] * 3;

            if (gameController.PositionOnBoard(x3, y3))
            {
                GameObject pieceAt3 = gameController.GetPosition(x3, y3);
                if (pieceAt3 != null && pieceAt3.GetComponent<MultiChessMan>().player != this.player)
                {
                    MovePlateAttackSpawn(x3, y3);
                }
            }
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


    // MultiChessMan.cs

    public void PointMovePlate(int x, int y)
    {
        if (gameController == null) return;
        if (!gameController.PositionOnBoard(x, y)) return;

        GameObject targetPiece = gameController.GetPosition(x, y);

        if (targetPiece == null) // 1. 칸이 비었으면 -> 일반 이동
        {
            MovePlateSpawn(x, y);
        }
        else // 2. 칸에 기물이 있으면
        {
            MultiChessMan targetCm = targetPiece.GetComponent<MultiChessMan>();
            if (targetCm.player != this.player) // 2-1. 적군이면 -> 공격
            {
                MovePlateAttackSpawn(x, y);
            }  
        }
    }

    // MultiChessMan.cs

    public void PawnMovePlate(int x, int y)
    {
        if (gameController == null) return;

        // 1. 한 칸 전진 (이동 또는 합성)
        if (gameController.PositionOnBoard(x, y))
        {
            GameObject targetPiece = gameController.GetPosition(x, y);
            if (targetPiece == null) // 1-1. 칸이 비었으면 -> 일반 이동
            {
                MovePlateSpawn(x, y);

                // 1-2. 두 칸 전진 (처음 움직이는 경우에만)
                if (pawnNeverMove)
                {
                    int direction = (player == "white") ? 1 : -1;
                    int twoStepY = y + direction;
                    if (gameController.PositionOnBoard(x, twoStepY) && gameController.GetPosition(x, twoStepY) == null)
                    {
                        MovePlateSpawn(x, twoStepY);
                    }
                }
            }
            else if (targetPiece.GetComponent<MultiChessMan>().player == this.player) // 1-3. 칸에 아군이 있으면 -> 합성 체크
            {
                if (gameController.GetFusionResultType(this.name, targetPiece.name) != null)
                {
                    MovePlateFusionSpawn(x, y);
                }
            }
        }


        // 2. 대각선 이동 (공격 또는 합성)
        int attackDirectionY = (player == "white") ? yBoard + 1 : yBoard - 1;
        for (int dx = -1; dx <= 1; dx += 2)
        {
            int attackX = xBoard + dx;

            if (gameController.PositionOnBoard(attackX, attackDirectionY))
            {
                GameObject targetPiece = gameController.GetPosition(attackX, attackDirectionY);
                if (targetPiece != null) // 칸에 기물이 있으면
                {
                    if (targetPiece.GetComponent<MultiChessMan>().player != this.player) // 2-1. 적군이면 -> 공격
                    {
                        MovePlateAttackSpawn(attackX, attackDirectionY);
                    }
                    else // 2-2. 아군이면 -> 합성 체크
                    {
                        if (gameController.GetFusionResultType(this.name, targetPiece.name) != null)
                        {
                            MovePlateFusionSpawn(attackX, attackDirectionY);
                        }
                    }
                }
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


    [PunRPC]
    public void RPC_SetShield(bool status)
    {
        // 상태가 변경될 때만 로그를 출력하도록 함
        if (this.hasShield != status)
        {
            this.hasShield = status;
            if (status)
            {
                Debug.Log($"<color=cyan>{this.name} 보호막 생성!</color>");
            }
            else
            {
                Debug.Log($"<color=orange>{this.name} 보호막 파괴!</color>");
            }
        }
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

    // MultiChessMan.cs 에 추가
    public void MovePlateFusionSpawn(int matrixX, int matrixY)
    {
        if (gameController == null) return;

        GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity);
        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();

        mpScript.isFusion = true; // 이 플레이트는 합성용이라고 표시
        mpScript.attack = false;  // 공격은 아님
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
        mpScript.SetupVisuals(gameController, matrixX, matrixY);

        
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

    // MultiChessMan.cs 에 추가

    void OnMouseDown()
    {
        Debug.Log("<color=red>CHESS PIECE가 클릭되었습니다!</color> 이름: " + this.gameObject.name);
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

    // 기물 합성 로직

    public void CheckAdjacentFusions()
    {
        if (gameController == null) return;

        // 상하좌우 4방향 좌표
        int[] xDirections = { 0, 0, 1, -1 };
        int[] yDirections = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int adjX = xBoard + xDirections[i];
            int adjY = yBoard + yDirections[i];

            if (gameController.PositionOnBoard(adjX, adjY))
            {
                GameObject targetPiece = gameController.GetPosition(adjX, adjY);
                // 인접한 칸에 아군 기물이 있다면
                if (targetPiece != null && targetPiece.GetComponent<MultiChessMan>().GetPlayer() == this.player)
                {
                    // '합성 레시피 북'을 확인해서 합성이 가능한지 알아본다
                    if (gameController.GetFusionResultType(this.name, targetPiece.name) != null)
                    {
                        // 합성이 가능하면, 그 아군 기물 위치에 초록색 Plate를 생성
                        MovePlateFusionSpawn(adjX, adjY);
                    }
                }
            }
        }
    }

    // 자신의 전방 보호막이 활성화 상태인지 외부에서 확인하는 함수
    public bool HasPhalanxShield()
    {
        return this.phalanxShieldActive;
    }

    // 전방 보호막이 소모되었을 때 호출될 RPC
    [PunRPC]
    public void RPC_ConsumePhalanxShield()
    {
        if (this.phalanxShieldActive)
        {
            this.phalanxShieldActive = false;
            // 그래픽이 없으므로, 대신 로그를 출력합니다.
            Debug.Log($"<color=orange>{this.name}의 전방 보호막이 파괴되었습니다!</color>");
        }
    }
}