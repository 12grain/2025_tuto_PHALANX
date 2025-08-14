using Photon.Pun;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.GraphicsBuffer;

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


    // 0~7 ü���� ��ǥ
    private int xBoard = -1;
    private int yBoard = -1;

    // Variable to keep track of "black" player of "white" player
    private string player;

    // Chessman.cs
    public string GetPlayer()
    {
        return player;
    }

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
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<MultiGame>();

        float t = game.tileSize;
        Vector2 origin = game.boardOrigin;

        float worldX = origin.x + xBoard * t;
        float worldY = origin.y + yBoard * t;

        // z ���� ���� ������ �����ϰų� -1�� ����
        float z = transform.position.z;
        transform.position = new Vector3(worldX, worldY, z);
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

    public void ExecuteMove(int targetX, int targetY, bool isAttack, bool isCastle)
    {
        // ���� ������ ���� �ƴϴ���, ���忡�� '��û'�� ���� �� �ִ�.
        // GameController�� �ִ� PhotonView�� �����´�.
        var gameControllerPV = controller.GetComponent<PhotonView>();
        if (gameControllerPV == null) return;

        // ������ ���� ViewID�� ã�´�. ������ -1
        int capturedID = -1;
        if (isAttack)
        {
            GameObject capturedPiece = controller.GetComponent<MultiGame>().GetPosition(targetX, targetY);
            if (capturedPiece != null)
            {
                capturedID = capturedPiece.GetComponent<PhotonView>().ViewID;
            }
        }

        // ����(MasterClient)���� �̵��� ��û�ϴ� RPC�� ������.
        // �ʿ��� ��� ������ �Ķ���ͷ� �Ѱ��ش�.
        gameControllerPV.RPC("RequestMovePiece", RpcTarget.MasterClient,
            photonView.ViewID, targetX, targetY, capturedID, isCastle);

        // �̵� �� �����Ǿ� �ִ� ��� MovePlate�� ��� �����Ѵ�.
        DestroyMovePlates();
    }



    [PunRPC]
    public void MoveTo(int targetX, int targetY)
    {
        // 1) ���� ��ġ ��ĭ ó��
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<MultiGame>();
        game.SetPositionEmpty(xBoard, yBoard);

        // 2) ��ǥ ����
        xBoard = targetX;
        yBoard = targetY;
        SetCoords();

        // 3) ���� �迭�� ����
        game.SetPosition(this.gameObject);
    }


    [PunRPC]
    public void DestroySelf()
    {
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

    // *** ��LineMovePlate�� : xIncrement, yIncrement ��������
    //    �� ĭ�� ���� ������ ��� ���� �� ��� �� ������ �����ݡ� ǥ��
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


    // *** ��PointMovePlate�� : ���� ��ǥ üũ �� �� ĭ�̸� MovePlate, ���̸� ���� MovePlate
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

        // 1. �� ĭ ����
        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);

            // 2. �� ĭ ���� (ó�� �����̴� ��쿡��)
            if (pawnNeverMove)
            {
                int twoStepY = y + direction;
                if (sc.PositionOnBoard(x, twoStepY) && sc.GetPosition(x, twoStepY) == null)
                {
                    MovePlateSpawn(x, twoStepY);
                }
            }
        }

        // 3. �밢�� ����
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


    // MultiChessMan.cs

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

    private void OnMouseUp()
    {
        var game = controller.GetComponent<MultiGame>();

        if (!controller.GetComponent<MultiGame>().IsGameOver() && controller.GetComponent<MultiGame>().GetCurrentPlayer() == player 
            && controller.GetComponent<MultiGame>().GetCurrentPlayer() == controller.GetComponent<MultiGame>().GetMyPlayerColor() )
        {

            DestroyMovePlates();

            InitiateMovePlates();

        }
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
}