using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiGame : MonoBehaviourPunCallbacks
{

    // MultiGame.cs 에 추가

    public void DebugPrintBoard()
    {
        // 현재 턴 정보를 함께 출력
        Debug.Log($"====== 현재 보드 상태 (턴: {currentPlayer}) ======");

        // 보드판을 위에서부터 아래로 (rank 8 -> 1) 출력
        for (int y = 7; y >= 0; y--)
        {
            string line = $"rank {y + 1}: "; // 각 줄의 시작 부분 (예: "rank 8: ")
            for (int x = 0; x < 8; x++)
            {
                GameObject piece = positions[x, y];

                if (piece == null)
                {
                    // 칸이 비어있으면 점(.)으로 표시
                    line += " . ";
                }
                else
                {
                    // 기물이 있으면, 색깔과 종류의 첫 글자로 축약해서 표시
                    MultiChessMan cm = piece.GetComponent<MultiChessMan>();
                    string color = (cm.GetPlayer() == "white") ? "W" : "B";
                    string type = piece.name.Split('_')[1].Substring(0, 1); // 예: "pawn" -> "P", "KNIGHT" -> "K"

                    line += $" {color}{type} ";
                }
            }
            // 완성된 한 줄을 콘솔에 출력
            Debug.Log(line);
        }
        Debug.Log("========================================");
    }
    public GameObject chesspiece;
    public Sprite boardSprite;
    public Sprite backgroundSprite;
    public PromotionManager promotionManager;
    public GameObject gameOverPanel;
    public Image victoryImage;
    public Image defeatImage;
    public Button returnToLobbyButton;
    public GameObject bastionPlatePrefab;

    // Positions and team for each chesspiece
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];


    private string myPlayerColor;
    private string currentPlayer;

    private bool gameOver = false;
    public bool isInteractionBlocked = false;
    private PhotonView pv;

    public float tileSize = 0.66f;                 
    public Vector2 boardOrigin = Vector2.zero;     
    public float boardWorldWidth = 0f;
    public float boardWorldHeight = 0f;

    //private bool turnChanged = false;


    void Awake()
    {
        myPlayerColor = PlayerColorManager.instance.GetMyToggle() ? "white" : "black";
    }

    void Start()
    {

        //��� ��ġ
        GameObject background = new GameObject("background");
        background.transform.position = new Vector3(0f, 0f, 0);
        var backgroundSr = background.AddComponent<SpriteRenderer>();
        backgroundSr.sprite = backgroundSprite;
        background.AddComponent<BoxCollider2D>();
        backgroundSr.sortingLayerName = "Board";
        backgroundSr.sortingOrder = 0;

        //board ��ġ
        GameObject board = new GameObject("Board");
        board.transform.position = new Vector3(0f, 0f, 0);
        var boardSr = board.AddComponent<SpriteRenderer>();
        boardSr.sprite = boardSprite;
        board.AddComponent<BoxCollider2D>();
        boardSr.sortingLayerName = "Board";
        boardSr.sortingOrder = 1;

        RecalculateBoardMetrics(); //���� ���ġ (���ο� �������� ���� ũ�⺯��)

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

        if (returnToLobbyButton != null)
        {
            returnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);
        }
    }

    public void OnReturnToLobbyClicked()
    {
        // 현재 입장해 있는 포톤 룸을 떠납니다.
        PhotonNetwork.LeaveRoom();
    }

    // LeaveRoom()이 성공적으로 완료되면 포톤이 자동으로 이 함수를 호출해줍니다.
    public override void OnLeftRoom()
    {
        // 로비 씬을 로드합니다. SceneManager.LoadScene 대신 PhotonNetwork.LoadLevel을 사용하는 것이 안전합니다.
        // "Lobby" 부분은 실제 로비 씬의 이름과 정확히 일치해야 합니다.
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public void RecalculateBoardMetrics()
    {
        GameObject boardObj = GameObject.Find("Board");
        if (boardObj == null) return;

        var sr = boardObj.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        // ������ ���� ���� ������
        boardWorldWidth = sr.bounds.size.x;
        boardWorldHeight = sr.bounds.size.y;

        // �� ĭ ũ��
        tileSize = boardWorldWidth / 8f;

        // (0,0) ĭ�� CENTER ��ǥ ���:
        // boardObj.transform.position �� ������ �߽�(����). 
        // ���� �ϴ� �𼭸� = center - (width/2, height/2)
        float left = boardObj.transform.position.x - boardWorldWidth / 2f;
        float bottom = boardObj.transform.position.y - boardWorldHeight / 2f;

        // (0,0) Ÿ���� center = left + tileSize/2, bottom + tileSize/2
        boardOrigin = new Vector2(left + tileSize * 0.5f, bottom + tileSize * 0.5f);

        Debug.Log($"Recalc: boardW={boardWorldWidth}, tileSize={tileSize}, boardOrigin={boardOrigin}");
    }

    // MultiGame.cs

    public GameObject Create(string name, int x, int y)
    {
        // 1. ��û���� �̸�("white_CHEVALIER")���� ���� �⹰ Ÿ��("CHEVALIER")�� �����մϴ�.
        string pieceType = name.Split('_')[1];
        string prefabToInstantiate;

        // 2. �⹰ Ÿ���� '�ռ� �⹰' �� �ϳ����� Ȯ���մϴ�.
        if (pieceType == "PHALANX" || pieceType == "TESTUDO" || pieceType == "CHEVALIER" || pieceType == "BASTION")
        {
            // 2-1. �ռ� �⹰�̶��, ��û���� �̸� �״���� �������� ����մϴ�.
            // ��: "white_CHEVALIER" -> "white_CHEVALIER.prefab"
            prefabToInstantiate = name;
        }
        else
        {
            // 2-2. �� ���� ��� �Ϲ� �⹰(rook, pawn ��)�̶��, ������ �⺻ �������� ����մϴ�.
            prefabToInstantiate = "MultiChesspiece";
        }

        // 3. ������ ������ �̸����� ��θ� �����, �ùٸ� �������� �����մϴ�.
        string prefabPath = "Pieces/" + prefabToInstantiate;
        Vector3 spawnPos = new Vector3(0f, 0f, 0);
        GameObject obj = PhotonNetwork.Instantiate(prefabPath, spawnPos, Quaternion.identity);

        // 4. ������ ��ü�� ���� ������ ���� RPC�� ȣ���մϴ�. (�� �κ��� ����)
        PhotonView pv = obj.GetComponent<PhotonView>();
        pv.RPC("SetupSprite", RpcTarget.AllBuffered, name, x, y);

        return obj;
    }

    // �̰ɷ� ���� ��ġ ����
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

    //GetCurrentPlayer()�� ���� �÷��̾ ��Ÿ�������� string ���� currentPlayer�� ��ȯ�մϴ�.
    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public string GetMyPlayerColor()
    {
        return myPlayerColor;
    }
    //IsGameOver()�� üũ����Ʈ����(�����������)������ ��Ÿ���� ���� bool ���� gameOver�� ��ȯ�մϴ�.
    public bool IsGameOver()
    {
        return gameOver;
    }

    //NextTurn()�� ���簡 ���� �����̸� �濡��, ���� �����̸� �鿡�� �ѱ�� �˰������� �����մϴ�.
    [PunRPC]//���ÿ� �����Ǿ�� �� �Լ�
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
        Debug.Log("�� �ѱ�");
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
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView attackerView = PhotonView.Find(attackerID);
        if (attackerView == null) return;
        MultiChessMan attackerCm = attackerView.GetComponent<MultiChessMan>();


        if (attackerCm.IsGarrisoned())
        {            
            int bastionToReleaseID = attackerCm.GetGarrisonedBastionID();
            PhotonView bastionView = PhotonView.Find(bastionToReleaseID);
            Debug.Log(bastionView);
            if (bastionView != null)
            {
                bastionView.RPC("RPC_SetVisible", RpcTarget.All, true);
                SetPosition(bastionView.gameObject);              
            }

            attackerView.RPC("RPC_SetGarrisonStatus", RpcTarget.All, false, -1);

        }

        if (isCastle)
        {
            photonView.RPC("RPC_ExecuteCastling", RpcTarget.All, attackerID, targetX);
        }
        else if (capturedID != -1) // 공격인 경우
        {
            PhotonView capturedView = PhotonView.Find(capturedID);
            if (capturedView != null)
            {
                MultiChessMan capturedCm = capturedView.GetComponent<MultiChessMan>();

                // 킹을 잡았는지 최우선으로 확인
                if (capturedCm.gameObject.name.Contains("king"))
                {
                    string winner = attackerCm.GetPlayer();
                    photonView.RPC("RPC_EndGame", RpcTarget.All, winner);
                    return; // 게임 종료
                }

                // 팔랑크스 전방 보호막 확인
                bool isPhalanxProtected = false;
                if (capturedCm.gameObject.name.Contains("PHALANX") && capturedCm.HasPhalanxShield())
                {
                    bool isSameFile = (attackerCm.GetXBoard() == capturedCm.GetXBoard());
                    string phalanxColor = capturedCm.GetPlayer();
                    int attackerY = attackerCm.GetYBoard();
                    int phalanxY = capturedCm.GetYBoard();
                    bool isFrontalAttack = (phalanxColor == "white" && attackerY > phalanxY) || (phalanxColor == "black" && attackerY < phalanxY);

                    if (isSameFile && isFrontalAttack)
                    {
                        isPhalanxProtected = true;
                        capturedView.RPC("RPC_ConsumePhalanxShield", RpcTarget.All);
                    }
                }

                // 개인 보호막 확인
                if (capturedCm.HasShield())
                {
                    capturedView.RPC("RPC_SetShield", RpcTarget.All, false);
                }
                // 팔랑크스 보호막이 발동했다면
                else if (isPhalanxProtected)
                {
                    // 아무것도 하지 않음 (공격자 이동 방지)
                }
                // 아무 보호막도 없다면 (공격 성공)
                else
                {
                    capturedView.RPC("DestroySelf", RpcTarget.AllBuffered);
                    attackerView.RPC("RPC_AnimateMove", RpcTarget.All, targetX, targetY, true);
                }
            }
        }
        else // ������ �ƴ� �Ϲ� �̵��� ���
            {
                attackerView.RPC("RPC_AnimateMove", RpcTarget.All, targetX, targetY, false);
            }
        attackerView.RPC("RPC_UpdateMovedStatus", RpcTarget.All);
        CallNextTurn();
        DebugPrintBoard();
    }


    // ĳ���� ���� ���θ� Ȯ���ϴ� �� �Լ�
    public bool CanCastle(int kingX, int kingY, bool isKingSide)
    {
        // 1. ŷ ���̵�(������) ĳ���� Ȯ��
        if (isKingSide)
        {
            // 1-1. ��ΰ� ������� Ȯ��
            if (positions[kingX + 1, kingY] != null || positions[kingX + 2, kingY] != null)
            {
                return false;
            }

            // 1-2. �ڳʿ� ���� �ִ��� Ȯ��
            GameObject rookObj = positions[kingX + 3, kingY];
            if (rookObj == null || !rookObj.name.Contains("rook"))
            {
                return false;
            }

            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>();
            if (rookCm == null || !rookCm.GetRookNeverMove())
            {
                // ���� ��ũ��Ʈ�� ���ų�, GetRookNeverMove()�� false�� ��ȯ�ϸ� ĳ���� �Ұ�
                return false;
            }
            return true; // ��� ������ ���
        }
        // 2. �� ���̵�(����) ĳ���� Ȯ��
        else
        {
            // 2-1. ��ΰ� ������� Ȯ��
            if (positions[kingX - 1, kingY] != null || positions[kingX - 2, kingY] != null || positions[kingX - 3, kingY] != null)
            {
                return false;
            }

            // 2-2. �ڳʿ� ���� �ִ��� Ȯ��
            GameObject rookObj = positions[kingX - 4, kingY];
            if (rookObj == null || !rookObj.name.Contains("rook"))
            {
                return false;
            }

            // 2-3. �� ���� ������ �� ������ Ȯ��
            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>();
            if (rookCm == null || !rookCm.GetRookNeverMove())
            {
                return false;
            }
            return true;
        }
    }


    // MultiGame.cs �� �ִ� RPC_ExecuteCastling �Լ�
    [PunRPC]
    public void RPC_ExecuteCastling(int kingID, int kingTargetX)
    {
        GameObject kingObj = PhotonView.Find(kingID).gameObject;
        MultiChessMan kingCm = kingObj.GetComponent<MultiChessMan>();

        int startKingX = kingCm.GetXBoard();
        int y = kingCm.GetYBoard();
        bool isKingSide = kingTargetX > startKingX;

        SetPositionEmpty(startKingX, y);

        kingObj.GetComponent<PhotonView>().RPC("RPC_AnimateMove", RpcTarget.All, kingTargetX, y, false);         

        SetPosition(kingObj);

        int rookStartX = isKingSide ? 7 : 0;
        int rookTargetX = isKingSide ? kingTargetX - 1 : kingTargetX + 1;

        GameObject rookObj = GetPosition(rookStartX, y);
        if (rookObj != null)
        {
            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>();
            SetPositionEmpty(rookStartX, y);

            rookObj.GetComponent<PhotonView>().RPC("RPC_AnimateMove", RpcTarget.All, rookTargetX, y, false);

            SetPosition(rookObj);
        }
    }

    // MultiGame.cs �� �߰� (���� RequestPawnMoveAndShowPromotionUI�� ����)

    [PunRPC]
    public void RPC_ExecutePromotion(int pawnViewID, int targetX, int targetY, int capturedID, string pieceType)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 1. 잡을 말이 있는지, 그리고 그 말이 킹인지 확인
        if (capturedID != -1)
        {
            PhotonView capturedView = PhotonView.Find(capturedID);
            if (capturedView != null)
            {
                // ▼▼▼ 여기에 게임 종료 확인 로직을 추가합니다! ▼▼▼
                if (capturedView.gameObject.name.Contains("king"))
                {
                    // 공격한 폰의 색깔이 승자
                    string winner = PhotonView.Find(pawnViewID).GetComponent<MultiChessMan>().GetPlayer();
                    photonView.RPC("RPC_EndGame", RpcTarget.All, winner);
                    return; // 게임이 끝났으므로 더 이상 프로모션을 진행하지 않음
                }

                // 킹이 아니라면, 평소처럼 기물을 파괴
                capturedView.RPC("DestroySelf", RpcTarget.AllBuffered);
            }
        }

        // 2. ���θ���� ���� ������ ������ �� �ı�
        GameObject pawnObj = PhotonView.Find(pawnViewID)?.gameObject;
        if (pawnObj == null) return;

        MultiChessMan pawnCm = pawnObj.GetComponent<MultiChessMan>();
        int startX = pawnCm.GetXBoard();
        int startY = pawnCm.GetYBoard();
        string playerColor = pawnCm.GetPlayer();

        SetPositionEmpty(startX, startY);
        PhotonNetwork.Destroy(pawnObj);

        // 3. ���ο� �⹰�� ���� '���� �ִ� ��ġ'�� ����
        string newPieceName = playerColor + "_" + pieceType;
        GameObject newPiece = Create(newPieceName, startX, startY);

        // 4. ������ �� �⹰�� �ִϸ��̼ǰ� �Բ� '��ǥ ��ġ'�� �̵��ϵ��� ����
        if (newPiece != null)
        {
            // ���� �� �κ��� �����Ǿ����ϴ�! ����
            // �� ���θ���� �����̾����� ���θ� isCapture ������ �����մϴ�.
            bool isCapture = (capturedID != -1);

            // AnimateMove�� ȣ���� �� isCapture ���� �״�� �����մϴ�.
            newPiece.GetComponent<PhotonView>().RPC("RPC_AnimateMove", RpcTarget.All, targetX, targetY, isCapture);
        }

        // 5. ���θ���� �������� ��ȣ�ۿ��� �ٽ� ���
        photonView.RPC("RPC_SetInteractionState", RpcTarget.All, false);

        // 6. ���� �ѱ�
        CallNextTurn();
    }


    

    [PunRPC]
    public void RPC_SetInteractionState(bool isBlocked)
    {
        this.isInteractionBlocked = isBlocked;
    }

    // MultiGame.cs �� RequestEnterBastion �Լ��� �� �ڵ�� ��ü
    [PunRPC]
    public void RequestEnterBastion(int movingPieceID, int bastionID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView movingPieceView = PhotonView.Find(movingPieceID);
        PhotonView bastionView = PhotonView.Find(bastionID);

        if (movingPieceView == null || bastionView == null) return;

        // 1. �ٽ�Ƽ���� ��ġ ������ ������
        int targetX = bastionView.GetComponent<MultiChessMan>().GetXBoard();
        int targetY = bastionView.GetComponent<MultiChessMan>().GetYBoard();

        // 2. �ٽ�Ƽ���� ������ �ʰ� �ϰ�, �ֵ��� �⹰ ID�� ����϶�� ����
        bastionView.RPC("RPC_SetVisible", RpcTarget.All, false);
        // (�ʿ��ϴٸ�, �ٽ�Ƽ���� isOccupied ���� ���µ� ���⼭ RPC�� ������Ʈ ����)

        // 3. ������ �⹰���� '�ֵ� ����'�� �Ǿ����� �˸� (� �ٽ�Ƽ�¿� ������ ID ����)
        movingPieceView.RPC("RPC_SetGarrisonStatus", RpcTarget.All, true, bastionID);

        // 4. ������ �⹰�� �ִϸ��̼ǰ� �Բ� �ٽ�Ƽ���� ��ġ�� �̵���Ŵ
        movingPieceView.RPC("RPC_AnimateMove", RpcTarget.All, targetX, targetY, false);

        // 5. ���� �ѱ�
        CallNextTurn();
    }

    // MultiGame.cs �� �߰�

    // �� �⹰ �̸��� �޾� �ռ� ����� ��ȯ�ϴ� '������ ��' �Լ�
    public string GetFusionResultType(string piece1Name, string piece2Name)
    {
        // �̸����� ���� �κ�("white_", "black_")�� �����ؼ� ���� �⹰ Ÿ�Ը� ��
        string type1 = piece1Name.Split('_')[1];
        string type2 = piece2Name.Split('_')[1];

        // �ȶ�ũ�� (�� + ��)
        if ((type1 == "pawn" && type2 == "pawn")) return "PHALANX";

        // �׽����� (�� + ���)
        if ((type1 == "pawn" && type2 == "bishop") || (type1 == "bishop" && type2 == "pawn")) return "TESTUDO";

        // ���߸��� (�� + ����Ʈ)
        if ((type1 == "pawn" && type2 == "knight") || (type1 == "knight" && type2 == "pawn")) return "CHEVALIER";

        // �ٽ�Ƽ�� (�� + ��)
        if ((type1 == "pawn" && type2 == "rook") || (type1 == "rook" && type2 == "pawn")) return "BASTION";

        // �ռ� �Ұ����� �����̸� null ��ȯ
        return null;
    }

    // MultiGame.cs �� �߰�
    [PunRPC]
    public void RequestFusion(int movingPieceID, int stationaryPieceID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject movingPiece = PhotonView.Find(movingPieceID)?.gameObject;
        GameObject stationaryPiece = PhotonView.Find(stationaryPieceID)?.gameObject;

        if (movingPiece == null || stationaryPiece == null) return;

        // 1. 합쳐지는 두 기물 중 주둔 중인 기물이 있는지 확인합니다.
        MultiChessMan movingCm = movingPiece.GetComponent<MultiChessMan>();
        MultiChessMan stationaryCm = stationaryPiece.GetComponent<MultiChessMan>();

        // 움직이는 기물이 주둔 중이었다면
        if (movingCm.IsGarrisoned())
        {
            int bastionID = movingCm.GetGarrisonedBastionID();
            PhotonView bastionView = PhotonView.Find(bastionID);
            if (bastionView != null)
            {
                bastionView.RPC("RPC_SetVisible", RpcTarget.All, true);
                SetPosition(bastionView.gameObject);
            }
        }
        // 가만히 있던 기물이 주둔 중이었다면 (이런 경우는 거의 없지만, 안전을 위해)
        if (stationaryCm.IsGarrisoned())
        {
            int bastionID = stationaryCm.GetGarrisonedBastionID();
            PhotonView bastionView = PhotonView.Find(bastionID);
            if (bastionView != null)
            {
                bastionView.RPC("RPC_SetVisible", RpcTarget.All, true);
                SetPosition(bastionView.gameObject);
            }
        }

        int targetX = stationaryCm.GetXBoard();
        int targetY = stationaryCm.GetYBoard();
        string playerColor = movingCm.GetPlayer();

        // 2. �ռ� ����� �̸� ���ϱ�
        string resultType = GetFusionResultType(movingPiece.name, stationaryPiece.name);
        string newPieceName = playerColor + "_" + resultType;

        // 3. ���� �� �⹰ ���忡�� ���� �� ��Ʈ��ũ �ı�
        SetPositionEmpty(movingCm.GetXBoard(), movingCm.GetYBoard());
        SetPositionEmpty(stationaryCm.GetXBoard(), stationaryCm.GetYBoard());
        PhotonNetwork.Destroy(movingPiece);
        PhotonNetwork.Destroy(stationaryPiece);

        // 4. ���ο� �ռ� �⹰ ����
        GameObject newPiece = Create(newPieceName, targetX, targetY);

        // ���� �׽������� ���� ��ȣ�� �ο� ���� ����
        if (resultType == "TESTUDO" && newPiece != null)
        {
            Debug.Log("�׽����� ����! �ֺ��� ��ȣ���� �ο��մϴ�.");
            string ownerColor = newPiece.GetComponent<MultiChessMan>().GetPlayer();

            // �ֺ� 8ĭ�� ��� Ȯ��
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue; 

                    int checkX = targetX + i;
                    int checkY = targetY + j;

                    if (PositionOnBoard(checkX, checkY))
                    {
                        GameObject pieceToShield = GetPosition(checkX, checkY);
                        if (pieceToShield != null && pieceToShield.GetComponent<MultiChessMan>().GetPlayer() == ownerColor)
                        {
                            pieceToShield.GetComponent<PhotonView>().RPC("RPC_SetShield", RpcTarget.All, true);
                        }
                    }
                }
            }
        }
        photonView.RPC("RPC_SetInteractionState", RpcTarget.All, false);

        CallNextTurn();
    }


    [PunRPC]
    public void RPC_PlacePieceOnBoard(int viewID)
    {
        PhotonView pieceView = PhotonView.Find(viewID);
        if (pieceView != null)
        {
            SetPosition(pieceView.gameObject);
        }
    }


    [PunRPC]
    public void RPC_StartFusionAnimation(int movingPieceID, int stationaryPieceID)
    {

        // 모든 클라이언트가 애니메이션 시작과 동시에 합성 효과음을 재생합니다.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayFusionSound();
        }

        PhotonView movingView = PhotonView.Find(movingPieceID);
        PhotonView stationaryView = PhotonView.Find(stationaryPieceID);

        if (movingView != null && stationaryView != null)
        {
            movingView.GetComponent<MultiChessMan>().StartFusionAnimationWith(stationaryView.gameObject);
        }
    }

    [PunRPC]
    public void RPC_EndGame(string winnerColor)
    {
        // 게임 상태를 종료로 변경
        gameOver = true;
        isInteractionBlocked = true; // 모든 입력 비활성화

        // 게임오버 패널 활성화
        gameOverPanel.SetActive(true);

        // 내가 승자인지 확인
        if (winnerColor == myPlayerColor)
        {
            victoryImage.gameObject.SetActive(true);
            defeatImage.gameObject.SetActive(false);
        }
        else // 내가 패자라면
        {
            victoryImage.gameObject.SetActive(false);
            defeatImage.gameObject.SetActive(true);
        }
    }

    // MultiGame.cs

    public void Update()
    {
        // 1. 마우스 왼쪽 버튼 클릭을 감지 (게임오버가 아닐 때만)
        if (!gameOver && Input.GetMouseButtonDown(0))
        {
            // 2. 카메라에서 마우스 위치로 보이지 않는 광선을 쏨
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            // 3. 광선에 무언가 맞았는지 확인
            if (hit.collider != null)
            {
                // 4. 만약 맞은 것이 '기물'도 아니고 '무브플레이트'도 아니라면 (즉, 보드나 배경이라면)
                if (hit.collider.GetComponent<MultiChessMan>() == null && hit.collider.GetComponent<MultiMovePlate>() == null)
                {
                    // 모든 MovePlate를 파괴
                    DestroyMovePlates();
                }
            }
            // 5. 광선에 아무것도 맞지 않았을 경우 (완전한 허공 클릭)
            else
            {
                // 이 경우에도 모든 MovePlate를 파괴
                DestroyMovePlates();
            }
        }

        // 기존의 게임 재시작 로직은 그대로 둡니다.
        //if (gameOver == true && Input.GetMouseButtonDown(0))
        //{
        //    gameOver = false;
        //    SceneManager.LoadScene("MultiTestGameScene");
        //}
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