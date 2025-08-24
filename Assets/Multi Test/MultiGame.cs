using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
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
    private string currentPlayer;

    private bool gameOver = false;
    public bool isInteractionBlocked = false;
    private PhotonView pv;

    public float tileSize = 0.66f;                 // �ڵ� ���� ������ �⺻��
    public Vector2 boardOrigin = Vector2.zero;     // 0,0 ĭ(=a1)�� ���� ��ǥ(����)
    public float boardWorldWidth = 0f;
    public float boardWorldHeight = 0f;

    //private bool turnChanged = false;


    void Awake()
    {
        myPlayerColor = PlayerColorManager.instance.GetMyToggle() ? "white" : "black";
        Debug.Log("�� �÷��̾� ����: " + myPlayerColor);
    }

    void Start()
    {

        //��� ��ġ
        GameObject background = new GameObject("background");
        background.transform.position = new Vector3(0f, 0f, 0);
        var backgroundSr = background.AddComponent<SpriteRenderer>();
        backgroundSr.sprite = backgroundSprite;
        backgroundSr.sortingLayerName = "Board";
        backgroundSr.sortingOrder = 0;

        //board ��ġ
        GameObject board = new GameObject("Board");
        board.transform.position = new Vector3(0f, 0f, 0);
        var boardSr = board.AddComponent<SpriteRenderer>();
        boardSr.sprite = boardSprite;
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

    // MultiGame.cs

    // MultiGame.cs �� RequestMovePiece �Լ�
    [PunRPC]
    public void RequestMovePiece(int attackerID, int targetX, int targetY, int capturedID, bool isCastle)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView attackerView = PhotonView.Find(attackerID);
        if (attackerView == null) return;
        MultiChessMan attackerCm = attackerView.GetComponent<MultiChessMan>();

        // ���� �ֵ� ���� ������ ���⼭ ���� ó���մϴ� ����
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

        if (isCastle)
        {
            photonView.RPC("RPC_ExecuteCastling", RpcTarget.All, attackerID, targetX);
        }
        else
        {
            if (capturedID != -1) // ������ ���
            {
                PhotonView capturedView = PhotonView.Find(capturedID);
                if (capturedView != null)
                {
                    MultiChessMan capturedCm = capturedView.GetComponent<MultiChessMan>();

                    // ���� �ȶ�ũ�� ��� ������ ���⿡ �߰��մϴ� ����
                    // 1. ���ݹ޴� �⹰�� '�ȶ�ũ��'�̰�, '���� ��ȣ��'�� Ȱ��ȭ �������� Ȯ��
                    if (capturedCm.gameObject.name.Contains("PHALANX") && capturedCm.HasPhalanxShield())
                    {
                        // 2. ������ ������ '����'���� Ȯ�� (���� ������ ������)
                        bool isSameFile = (attackerCm.GetXBoard() == capturedCm.GetXBoard());
                        string phalanxColor = capturedCm.GetPlayer();
                        int attackerY = attackerCm.GetYBoard();
                        int phalanxY = capturedCm.GetYBoard();

                        bool isFrontalAttack = (phalanxColor == "white" && attackerY > phalanxY) ||
                                               (phalanxColor == "black" && attackerY < phalanxY);

                        // 3. ���� '���� ������ �� ���� ����'�� �´ٸ�, ������ ���� ��ȣ���� �Ҹ�
                        if (isSameFile && isFrontalAttack)
                        {
                            Debug.Log("�ȶ�ũ���� ���� ������ ����߽��ϴ�!");
                            capturedView.RPC("RPC_ConsumePhalanxShield", RpcTarget.All);
                            // �����ڴ� �̵����� �ʰ� �ϸ� �Ѿ�ϴ�.
                        }
                        else // ���� ������ �ƴ϶�� (�밢��, ����, �Ĺ�)
                        {
                            // ��ȣ���� �ҿ�����Ƿ�, �Ϲ� �⹰ó�� �׳� �����ϴ�.
                            capturedView.RPC("DestroySelf", RpcTarget.AllBuffered);
                            attackerView.RPC("RPC_AnimateMove", RpcTarget.All, targetX, targetY, true);
                        }
                    }
                    // 4. ���ݹ޴� �⹰�� �ȶ�ũ���� �ƴϰų�, ��ȣ���� ���� ���
                    else if (capturedCm.HasShield()) // �׽������� ���� ��ȣ�� Ȯ��
                    {
                        capturedView.RPC("RPC_SetShield", RpcTarget.All, false);
                    }
                    else // �ƹ� ��ȣ���� ���� �Ϲ� �⹰
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
        }

        attackerView.RPC("RPC_UpdateMovedStatus", RpcTarget.All);
        CallNextTurn();
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

        // 1. ŷ �̵�
        SetPositionEmpty(startKingX, y);

        // ���� �� �κ��� �����Ǿ����ϴ� ����
        kingObj.GetComponent<PhotonView>().RPC("RPC_AnimateMove", RpcTarget.All, kingTargetX, y, false);         // 2. �ٲ� ���� ��ǥ�� ���� ���� ��ġ�� �ű�� �Ѵ�.

        SetPosition(kingObj);

        // 2. �� ã�� �� �̵�
        int rookStartX = isKingSide ? 7 : 0;
        int rookTargetX = isKingSide ? kingTargetX - 1 : kingTargetX + 1;

        GameObject rookObj = GetPosition(rookStartX, y);
        if (rookObj != null)
        {
            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>();
            SetPositionEmpty(rookStartX, y);

            // ���� �赵 �Ȱ��� �����Ǿ����ϴ� ����
            rookObj.GetComponent<PhotonView>().RPC("RPC_AnimateMove", RpcTarget.All, rookTargetX, y, false);

            SetPosition(rookObj);
        }
    }

    // MultiGame.cs �� �߰� (���� RequestPawnMoveAndShowPromotionUI�� ����)

    [PunRPC]
    public void RPC_ExecutePromotion(int pawnViewID, int targetX, int targetY, int capturedID, string pieceType)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 1. ���� ���� ������ ���� �ı�
        if (capturedID != -1)
        {
            PhotonView.Find(capturedID)?.RPC("DestroySelf", RpcTarget.AllBuffered);
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


    // ��û 2 ó��: �÷��̾��� ������ �޾� ���� ���θ���� �����ϰ� ���� �ѱ�
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

        // 1. ���� ���� ���� �迭���� �����ϰ� ��Ʈ��ũ���� �ı�
        SetPositionEmpty(x, y);
        PhotonNetwork.Destroy(pawnObj);

        // 2. �� �⹰ ����
        string newPieceName = playerColor + "_" + pieceType;
        Create(newPieceName, x, y); // ������ Create �Լ��� ��Ȱ��

        // ���� ���θ���� ��������, �ٽ� ��ȣ�ۿ��� ����ϵ��� RPC ȣ�� ����
        photonView.RPC("RPC_SetInteractionState", RpcTarget.All, false);

        // 3. ��� �۾��� �������Ƿ� ���� �ѱ�
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

        // 1. �ռ��� �⹰���� ���� ��������
        MultiChessMan movingCm = movingPiece.GetComponent<MultiChessMan>();
        MultiChessMan stationaryCm = stationaryPiece.GetComponent<MultiChessMan>();
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
                    if (i == 0 && j == 0) continue; // �ڱ� �ڽ��� ����

                    int checkX = targetX + i;
                    int checkY = targetY + j;

                    if (PositionOnBoard(checkX, checkY))
                    {
                        GameObject pieceToShield = GetPosition(checkX, checkY);
                        // �װ��� �⹰�� �ְ�, �� �⹰�� '�Ʊ�'�� ���
                        if (pieceToShield != null && pieceToShield.GetComponent<MultiChessMan>().GetPlayer() == ownerColor)
                        {
                            // ��ȣ���� �ο��϶�� RPC�� ����
                            pieceToShield.GetComponent<PhotonView>().RPC("RPC_SetShield", RpcTarget.All, true);
                        }
                    }
                }
            }
        }
        photonView.RPC("RPC_SetInteractionState", RpcTarget.All, false);

        // 5. �� �ѱ��
        CallNextTurn();
    }


    // MultiGame.cs �� ���� �߰�
    [PunRPC]
    public void RPC_PlacePieceOnBoard(int viewID)
    {
        PhotonView pieceView = PhotonView.Find(viewID);
        if (pieceView != null)
        {
            SetPosition(pieceView.gameObject);
        }
    }

    // MultiGame.cs �� �߰�

    [PunRPC]
    public void RPC_StartFusionAnimation(int movingPieceID, int stationaryPieceID)
    {
        PhotonView movingView = PhotonView.Find(movingPieceID);
        PhotonView stationaryView = PhotonView.Find(stationaryPieceID);

        if (movingView != null && stationaryView != null)
        {
            // �����̴� �⹰����, ������ �ִ� �⹰�� ���� �ִϸ��̼��� �����϶�� ����
            movingView.GetComponent<MultiChessMan>().StartFusionAnimationWith(stationaryView.gameObject);
        }
    }


    //Update() ����Ƽ���� �����ϴ� ����Ƽ �̺�Ʈ �޼ҵ�� �������� ����ɶ����� ȣ��Ǵ� �޼ҵ� �Դϴ�
    //gameOver�� true�̰� ���콺 ��Ŭ�� �����϶� ������ ������մϴ�.
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