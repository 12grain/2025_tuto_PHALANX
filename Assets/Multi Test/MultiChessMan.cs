using Photon.Pun;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class MultiChessMan : MonoBehaviourPunCallbacks, IPunObservable 
{


    // References
    public GameObject controller;
    private MultiGame gameController; // ��ũ��Ʈ ������ ������ ����
    public GameObject movePlate;

    //���� �ȿ����� ��� �� ĭ �̵��� �� �ֵ��� �ϴ� �ο� ����
    private bool pawnNeverMove = true;
    private bool kingNeverMove = true;
    private bool rookNeverMove = true; // ���� ���� ���� �߰�
    private bool isGarrisoned = false;
    private bool hasShield = false;
    private bool phalanxShieldActive = true;
    private int garrisonedBastionID = -1;


    // 0~7 ü���� ��ǥ
    private int xBoard = -1;
    private int yBoard = -1;

    // Variable to keep track of "black" player of "white" player
    private string player;


    [Header("Animation Settings")]
    public float moveDuration = 0.3f; // ���� �̵��ϴ� �� �ɸ��� �ð� (��)
    public float hopHeight = 0.5f;    // ����Ʈ�� �����ϴ� ����

    // Chessman.cs
    public string GetPlayer()
    {
        return player;
    }

    // MultiChessMan.cs �� �߰�
    public bool HasShield()
    {
        return this.hasShield;
    }

    // MultiGame.cs ������ capturedCm.hasShield ��� capturedCm.HasShield() �� ���

    // �� ŷ�� ������ ���� ������ �ܺο��� Ȯ���� �� �ְ� ���ִ� �Լ�
    public bool GetKingNeverMove()
    {
        return this.kingNeverMove;
    }

    // �� ���� ������ ���� ������ �ܺο��� Ȯ���� �� �ְ� ���ִ� �Լ�
    public bool GetRookNeverMove()
    {
        return this.rookNeverMove;
    }

    // MultiChessMan.cs �� �߰�
    [PunRPC]
    public void RPC_UpdateMovedStatus()
    {
        // ���� �� ���� ó�� ������ ���� �ƴϰ� ��
        this.pawnNeverMove = false;
        this.kingNeverMove = false;
        this.rookNeverMove = false;
    }

    // MultiChessMan.cs Ŭ���� ���ο� �߰�
    // �� �⹰�� �ֵ� �������� �ܺο��� Ȯ���ϱ� ���� �Լ�
    public bool IsGarrisoned()
    {
        return isGarrisoned;
    }

    // � �ٽ�Ƽ�¿� �ֵ��ߴ��� �ܺο��� Ȯ���ϱ� ���� �Լ�
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

    // �ٽ�Ƽ�� ����: �ڽ��� ����ų� ��Ÿ���� �ϴ� RPC
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

        // ���� ���⿡ ���ο� ��������Ʈ �ε� �ڵ� �߰� ����
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
            // ���� ��ã���� ���� �޽����� Ȯ���ϰ� ���ܼ� ������� �����ϴ�.
            Debug.LogError("!!!!!!!! GameController ������Ʈ�� ã�� �� �����ϴ�! Tag�� ��Ȯ���� Ȯ���ϼ���. !!!!!!!!");
        }

        // ... ������ �ٸ� Awake() �ڵ� (��������Ʈ �ε� ��)�� �־��ٸ� �״�� �Ӵϴ� ...

    }

    // �� �ʱ�ȭ: ���� ��Ʈ�ѷ� ã��, ��������Ʈ & �÷��̾� ����, ȭ�� ��ġ �̵�
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
        // ������ Awake()���� ã�Ƶ� gameController ������ ����մϴ�.
        if (gameController == null)
        {
            Debug.LogError($"SetCoords Error: gameController is not assigned on {this.name}!");
            return;
        }

        float t = gameController.tileSize;
        Vector2 origin = gameController.boardOrigin;

        float worldX = origin.x + xBoard * t;
        float worldY = origin.y + yBoard * t;

        // z ���� -1�� �����Ͽ� �������� ���Դϴ�.
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

        // ��� Ŭ���̾�Ʈ���� ���� �迭�� ���
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<MultiGame>();
        game.SetPosition(this.gameObject);

        // ���� ������ ���� ���� �ڵ� �߰� ����
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Pieces"; // "Pieces" ���� �׸����� ����
            sr.sortingOrder = 1; // �׳� 1�� �θ� �˴ϴ�. sorting layer�� ��� ������ �̹� pieces�� 2���� ����. 2���� order 1�̴�.
        }


        // === ������ �ڵ� ���� ===
        if (sr != null && sr.sprite != null)
        {
            // sprite ���� �� (���� ����) - bounds�� transform.localScale�� �̹� �ݿ���
            float spriteWidth = sr.sprite.bounds.size.x;

            // ������ tileSize ���
            float tile = game.tileSize-0.1f;

            // ĭ ũ�⿡ ���� sprite�� ������ ����(��: 0.8 => ĭ�� 80% �ʺ�� ����)
            float fillRatio = 0.65f;

            // ���ϴ� ���� �� = tile * fillRatio
            float desiredWorldWidth = tile * fillRatio;

            // ���� sprite.bounds.size.x �� '���� 1.0 scale'������ ��������, 
            // sr.sprite.bounds.size.x * transform.localScale.x = ���� world ��.
            // �츮�� transform.localScale�� �ٽ� ������ ���̹Ƿ� �Ʒ�ó�� ���:
            float originalSpriteWidth = sr.sprite.bounds.size.x; // units at scale=1

            if (originalSpriteWidth > 0.0001f)
            {
                float scale = desiredWorldWidth / originalSpriteWidth;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        // ���������� ���� �迭�� ��� �� coords ����
        SetCoords();
        var gameRef = GameObject.FindGameObjectWithTag("GameController").GetComponent<MultiGame>();
        gameRef.SetPosition(this.gameObject);

        // ���� ���̾� ���� (������)
        if (sr != null)
        {
            sr.sortingLayerName = "Pieces";
            sr.sortingOrder = 2;
        }
    }

    // 2. MovePlate �ı� �Լ� ����
    public void DestroyMovePlates()
    {
        // �±׷� ã�Ƽ� �����ϰ� �ı�
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

        // Ư�� �̵�(�ռ� �Ǵ� �ٽ�Ƽ�� ����) ó��
        if (isSpecialMove)
        {
            GameObject targetPiece = gameController.GetPosition(targetX, targetY);
            if (targetPiece != null)
            {
                int targetID = targetPiece.GetComponent<PhotonView>().ViewID;
                if (targetPiece.name.Contains("BASTION")) // ��ǥ�� �ٽ�Ƽ���̸�
                {
                    gameController.GetComponent<PhotonView>().RPC("RequestEnterBastion", RpcTarget.MasterClient, photonView.ViewID, targetID);
                }
                else // �� �ܴ� �ռ�
                {
                    gameController.GetComponent<PhotonView>().RPC("RPC_StartFusionAnimation", RpcTarget.All, photonView.ViewID, targetID);
                }
            }
        }
        // ���� "���� �ռ��� �ƴ϶��" �̶�� 'else if'�� �����ݴϴ� ����
        else
        {
            // 2. ���θ�� �������� ��Ȯ�ϰ� Ȯ��
            bool isPromotion = (this.name.Contains("pawn") &&
                               ((player == "white" && targetY == 7) || (player == "black" && targetY == 0)));

            if (isPromotion)
            {
                // 3. ���θ���� ���
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
                // 4. �� �� ��� �Ϲ� �̵�/ĳ������ ���
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

        // ���� "SetPositionEmpty" �ڵ带 �ٽ� �߰��մϴ�! ����
        if (gameController != null)
        {
            // ��� Ŭ���̾�Ʈ�� �ڽ��� �������� ���� ���� �ڸ��� ����.
            gameController.SetPositionEmpty(this.xBoard, this.yBoard);
        }

        // 1. �ڽ��� '���ο�' ���� ��ġ�� ������Ʈ
        xBoard = targetX;
        yBoard = targetY;
        if (gameController != null)
        {
            gameController.SetPosition(this.gameObject); // �� ĭ�� �ڽ��� ���
        }

        // 2. ��ǥ ���� ��ǥ ��� �� �ִϸ��̼� ���� (�� �κ��� ����)
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

    // �ռ� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator AnimateFusion(GameObject otherPiece)
    {
        // �ִϸ��̼��� ����Ǵ� ������ �ٸ� �Է��� ����
        if (gameController != null)
        {
            gameController.isInteractionBlocked = true;
        }

        Vector3 startPosThis = this.transform.position;
        Vector3 startPosOther = otherPiece.transform.position;
        Vector3 targetPos = otherPiece.transform.position; // ��ᰡ �Ǵ� �⹰ ��ġ�� ����

        float elapsedTime = 0f;
        float fusionAnimDuration = 0.5f; // �ռ� �ִϸ��̼� �ð�

        while (elapsedTime < fusionAnimDuration)
        {
            // �� �⹰�� ��ǥ ��ġ�� ���� �ٰ���
            this.transform.position = Vector3.Lerp(startPosThis, targetPos, elapsedTime / fusionAnimDuration);
            otherPiece.transform.position = Vector3.Lerp(startPosOther, targetPos, elapsedTime / fusionAnimDuration);

            elapsedTime += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        // �ִϸ��̼��� ���� ��, ��û�� ���´� Ŭ���̾�Ʈ�� ���� �ռ��� ��û��
        if (photonView.IsMine)
        {
            int stationaryPieceID = otherPiece.GetComponent<PhotonView>().ViewID;
            gameController.GetComponent<PhotonView>().RPC("RequestFusion", RpcTarget.MasterClient, photonView.ViewID, stationaryPieceID);
        }
    }

    // �ִϸ��̼��� ���۽�Ű�� ���� �Լ� (RPC�� ȣ��� ����)
    public void StartFusionAnimationWith(GameObject otherPiece)
    {
        StartCoroutine(AnimateFusion(otherPiece));
    }


    [PunRPC]
    public void DestroySelf()
    {
        SoundManager.Instance.PlayKillSound();

        // 1) ��� Ŭ���̾�Ʈ���� ���� �迭 ����
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<MultiGame>();
        game.SetPositionEmpty(xBoard, yBoard);

        // 2) ���ʸ� �ı�
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }


    public void InitiateMovePlates()
    {
        // ���� �Լ� �� ���� ���� �ռ� üũ ������ �߰��մϴ� ����
        CheckAdjacentFusions();


        // ���� �Լ� �� ���� �ٽ�Ƽ�� ���� Ȯ�� ������ �߰��մϴ� ����
        if (this.isGarrisoned)
        {
            // ���� �ֵ� ���¶��, �ٸ� ��� �������� �����ϰ� ���� ��ó���� �����δ�.
            Debug.Log(this.name + "�� �ٽ�Ƽ�¿� �ֵ� ��: ��ó�� �����Դϴ�!");
            LineMovePlate(1, 0);
            LineMovePlate(-1, 0);
            LineMovePlate(0, 1);
            LineMovePlate(0, -1);
            return; // ���⼭ �Լ��� ������, �Ʒ��� switch���� ������� �ʵ��� ��
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
            // ���� ���ο� �⹰���� case �߰� ����
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

    // *** ��LineMovePlate�� : xIncrement, yIncrement ��������
    //    �� ĭ�� ���� ������ ��� ���� �� ��� �� ������ �����ݡ� ǥ��
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
                if (targetCm.player != this.player) // �����̸� -> ����
                {
                    MovePlateAttackSpawn(x, y);
                }
            }
        }
    }
    // *** ��LMovePlate�� : ����Ʈ �̵� 
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
    // *** ��SurroundMovePlate�� : �ֺ� 8ĭ - ŷ �̵�
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
    // MultiChessMan.cs �� CastleingMovePlate �Լ�
    public void CastleingMovePlate()
    {
        if (!kingNeverMove) return;

        // gameController�� Awake���� ĳ���� �״ٰ� ����
        if (gameController.CanCastle(xBoard, yBoard, true)) // ŷ ���̵�(������) üũ
        {
            CastlingPlateSpawn(xBoard + 2, yBoard);
        }
        if (gameController.CanCastle(xBoard, yBoard, false)) // �� ���̵�(����) üũ
        {
            CastlingPlateSpawn(xBoard - 2, yBoard);
        }
    }


    // MultiChessMan.cs �� �߰�

    public void PhalanxMovePlate()
    {
        if (gameController == null) return;

        // �÷��̾� ���� ���� '��' ������ ����
        int forwardDirection = (player == "white") ? 1 : -1;

        // --- ��Ģ 1: ���� 1ĭ �̵� ---
        int moveX = xBoard;
        int moveY = yBoard + forwardDirection;

        // �̵��� ĭ�� ���� ���̰� ������� ���� �̵� ����
        if (gameController.PositionOnBoard(moveX, moveY) && gameController.GetPosition(moveX, moveY) == null)
        {
            MovePlateSpawn(moveX, moveY);
        }

        // --- ��Ģ 2: 3���� ���� (����, ��, ��) ---
        // ������ ��ǥ���� ���� �迭
        int[] attackXCoords = { xBoard, xBoard - 1, xBoard + 1 };
        int[] attackYCoords = { yBoard + forwardDirection, yBoard, yBoard };

        // �� ���� ������ ������� Ȯ��
        for (int i = 0; i < 3; i++)
        {
            int currentAttackX = attackXCoords[i];
            int currentAttackY = attackYCoords[i];

            // ������ ĭ�� ���� ���� ���� Ȯ��
            if (gameController.PositionOnBoard(currentAttackX, currentAttackY))
            {
                GameObject pieceToAttack = gameController.GetPosition(currentAttackX, currentAttackY);
                // �װ��� �⹰�� �ְ�, �� �⹰�� '��'�� ��� ���� ����
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

        // 4���� ���� ���� (��, ��, ��, ��)
        int[] xDirections = { 0, 0, 1, -1 };
        int[] yDirections = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            // --- �Ϲ� �̵� (1ĭ & 2ĭ, ��� Ȯ�� �ʿ�) ---
            int x1 = xBoard + xDirections[i];
            int y1 = yBoard + yDirections[i];

            if (gameController.PositionOnBoard(x1, y1))
            {
                // 1ĭ ���� PointMovePlate�� Ȯ�� (�������, ������, �Ʊ����� �˾Ƽ� �Ǵ�)
                PointMovePlate(x1, y1);

                // 2ĭ ���� 1ĭ ���� ������� ���� �� �� ����
                if (gameController.GetPosition(x1, y1) == null)
                {
                    int x2 = xBoard + xDirections[i] * 2;
                    int y2 = yBoard + yDirections[i] * 2;
                    if (gameController.PositionOnBoard(x2, y2))
                    {
                        // 2ĭ �յ� PointMovePlate�� Ȯ��
                        PointMovePlate(x2, y2);
                    }
                }
            }

            // --- ���� ���� (3ĭ, �߰� ��ֹ� ����) ---
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
        // gameController�� Awake()���� ĳ���صξ��ٰ� �����մϴ�.
        if (gameController == null)
        {
            Debug.LogError("Game Controller�� ��� CastlingPlate�� ������ �� �����ϴ�.");
            return;
        }

        // 1. �ϵ��ڵ��� ��ǥ ����� ��� �����ϰ�, �⺻������ �����մϴ�.
        GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity);

        // 2. ������ ��ü�� ��ũ��Ʈ�� �����ɴϴ�.
        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();

        // 3. ������ ���� ��ǥ�� �����մϴ�.
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(targetX, targetY);

        // 4. �ڡڡ� �� �÷���Ʈ�� ĳ���������� ǥ���մϴ�. �ڡڡ�
        mpScript.isCastling = true;

        // 5. ���� ��� �ð��� ó���� SetupVisuals �Լ����� �ñ�ϴ�.
        mpScript.SetupVisuals(gameController, targetX, targetY);
    }


    // MultiChessMan.cs

    public void PointMovePlate(int x, int y)
    {
        if (gameController == null) return;
        if (!gameController.PositionOnBoard(x, y)) return;

        GameObject targetPiece = gameController.GetPosition(x, y);

        if (targetPiece == null) // 1. ĭ�� ������� -> �Ϲ� �̵�
        {
            MovePlateSpawn(x, y);
        }
        else // 2. ĭ�� �⹰�� ������
        {
            MultiChessMan targetCm = targetPiece.GetComponent<MultiChessMan>();
            if (targetCm.player != this.player) // 2-1. �����̸� -> ����
            {
                MovePlateAttackSpawn(x, y);
            }  
        }
    }

    // MultiChessMan.cs

    public void PawnMovePlate(int x, int y)
    {
        if (gameController == null) return;

        // 1. �� ĭ ���� (�̵� �Ǵ� �ռ�)
        if (gameController.PositionOnBoard(x, y))
        {
            GameObject targetPiece = gameController.GetPosition(x, y);
            if (targetPiece == null) // 1-1. ĭ�� ������� -> �Ϲ� �̵�
            {
                MovePlateSpawn(x, y);

                // 1-2. �� ĭ ���� (ó�� �����̴� ��쿡��)
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
            else if (targetPiece.GetComponent<MultiChessMan>().player == this.player) // 1-3. ĭ�� �Ʊ��� ������ -> �ռ� üũ
            {
                if (gameController.GetFusionResultType(this.name, targetPiece.name) != null)
                {
                    MovePlateFusionSpawn(x, y);
                }
            }
        }


        // 2. �밢�� �̵� (���� �Ǵ� �ռ�)
        int attackDirectionY = (player == "white") ? yBoard + 1 : yBoard - 1;
        for (int dx = -1; dx <= 1; dx += 2)
        {
            int attackX = xBoard + dx;

            if (gameController.PositionOnBoard(attackX, attackDirectionY))
            {
                GameObject targetPiece = gameController.GetPosition(attackX, attackDirectionY);
                if (targetPiece != null) // ĭ�� �⹰�� ������
                {
                    if (targetPiece.GetComponent<MultiChessMan>().player != this.player) // 2-1. �����̸� -> ����
                    {
                        MovePlateAttackSpawn(attackX, attackDirectionY);
                    }
                    else // 2-2. �Ʊ��̸� -> �ռ� üũ
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

    // ������ MovePlate �������� Instantiate �ϴ� �� �޼ҵ�
    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        if (gameController == null)
        {
            // ������ ����� ��� �ڵ�
            Debug.LogError("Game Controller�� �Ҵ���� �ʾ� MovePlate�� ������ �� �����ϴ�.");
            return;
        }

        // 1. �������� �⺻������ ����
        GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity);

        // 2. ������ ��ü�� ��ũ��Ʈ�� ������
        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();

        // 3. ������ ��ǥ ����
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);

        // 4. �ڡڡ� ���� ���� �ð�ȭ �Լ� ȣ�� �ڡڡ�
        // ���� ��Ʈ�ѷ��� ���� ��ǥ�� �Ѱ��ָ� �˾Ƽ� ��ġ/ũ�⸦ ������
        mpScript.SetupVisuals(gameController, matrixX, matrixY);
    }


    [PunRPC]
    public void RPC_SetShield(bool status)
    {
        // ���°� ����� ���� �α׸� ����ϵ��� ��
        if (this.hasShield != status)
        {
            this.hasShield = status;
            if (status)
            {
                Debug.Log($"<color=cyan>{this.name} ��ȣ�� ����!</color>");
            }
            else
            {
                Debug.Log($"<color=orange>{this.name} ��ȣ�� �ı�!</color>");
            }
        }
    }



    private void OnMouseUp()
    {
        var game = controller.GetComponent<MultiGame>();


        // ������ Awake()���� ĳ���ص� gameController ������ ����ؾ� �մϴ�.
        if (gameController == null) return;

        // ���� !gameController.isInteractionBlocked ������ �� if���� �߰��մϴ� ����
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

        // 1. ������ ����
        GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity);

        // 2. ��ũ��Ʈ ��������
        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();

        // 3. �ڡڡ� attack �÷��� ���� (���� �߿�!) �ڡڡ�
        mpScript.attack = true;

        // 4. ������ ���� �� �ð�ȭ �Լ� ȣ��
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
        mpScript.SetupVisuals(gameController, matrixX, matrixY);
    }

    // MultiChessMan.cs �� �߰�
    public void MovePlateFusionSpawn(int matrixX, int matrixY)
    {
        if (gameController == null) return;

        GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity);
        MultiMovePlate mpScript = mp.GetComponent<MultiMovePlate>();

        mpScript.isFusion = true; // �� �÷���Ʈ�� �ռ����̶�� ǥ��
        mpScript.attack = false;  // ������ �ƴ�
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
        mpScript.SetupVisuals(gameController, matrixX, matrixY);

        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // �����ڰ� �����͸� ����
            stream.SendNext(xBoard);
            stream.SendNext(yBoard);
            stream.SendNext(transform.position);
        }
        else
        {
            // �ٸ� Ŭ���̾�Ʈ�� ������ ����
            xBoard = (int)stream.ReceiveNext();
            yBoard = (int)stream.ReceiveNext();
            Vector3 pos = (Vector3)stream.ReceiveNext();

            // ��ǥ�� ��ġ ����
            transform.position = pos;
        }
    }

    private IEnumerator AnimateSlide(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            // �ð��� ���� ���� ��ġ�� ��ǥ ��ġ ���̸� �ε巴�� ����(Lerp)
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        // �ִϸ��̼��� ���� �� ��Ȯ�� ��ġ�� ����
        transform.position = targetPosition;
    }

    // MultiChessMan.cs �� �߰�

    void OnMouseDown()
    {
        Debug.Log("<color=red>CHESS PIECE�� Ŭ���Ǿ����ϴ�!</color> �̸�: " + this.gameObject.name);
    }

    // 2. ����Ʈ �̵� �ִϸ��̼� (ȩ/����)
    private IEnumerator AnimateHop(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            // x, z���� �������� �����̵�, y��(����)�� ���Ʒ��� ������ ���� ȿ���� ��
            float progress = elapsedTime / moveDuration;
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            currentPos.y += hopHeight * Mathf.Sin(progress * Mathf.PI); // Sin �Լ��� �ε巯�� ��ũ ����

            transform.position = currentPos;
            elapsedTime += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        // �ִϸ��̼��� ���� �� ��Ȯ�� ��ġ�� ����
        transform.position = targetPosition;
    }

    // �⹰ �ռ� ����

    public void CheckAdjacentFusions()
    {
        if (gameController == null) return;

        // �����¿� 4���� ��ǥ
        int[] xDirections = { 0, 0, 1, -1 };
        int[] yDirections = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int adjX = xBoard + xDirections[i];
            int adjY = yBoard + yDirections[i];

            if (gameController.PositionOnBoard(adjX, adjY))
            {
                GameObject targetPiece = gameController.GetPosition(adjX, adjY);
                // ������ ĭ�� �Ʊ� �⹰�� �ִٸ�
                if (targetPiece != null && targetPiece.GetComponent<MultiChessMan>().GetPlayer() == this.player)
                {
                    // '�ռ� ������ ��'�� Ȯ���ؼ� �ռ��� �������� �˾ƺ���
                    if (gameController.GetFusionResultType(this.name, targetPiece.name) != null)
                    {
                        // �ռ��� �����ϸ�, �� �Ʊ� �⹰ ��ġ�� �ʷϻ� Plate�� ����
                        MovePlateFusionSpawn(adjX, adjY);
                    }
                }
            }
        }
    }

    // �ڽ��� ���� ��ȣ���� Ȱ��ȭ �������� �ܺο��� Ȯ���ϴ� �Լ�
    public bool HasPhalanxShield()
    {
        return this.phalanxShieldActive;
    }

    // ���� ��ȣ���� �Ҹ�Ǿ��� �� ȣ��� RPC
    [PunRPC]
    public void RPC_ConsumePhalanxShield()
    {
        if (this.phalanxShieldActive)
        {
            this.phalanxShieldActive = false;
            // �׷����� �����Ƿ�, ��� �α׸� ����մϴ�.
            Debug.Log($"<color=orange>{this.name}�� ���� ��ȣ���� �ı��Ǿ����ϴ�!</color>");
        }
    }
}