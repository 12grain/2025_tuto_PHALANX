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
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

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
    private int whiteActor = -1;
    private int blackActor = -1;
    private void TryAssignColors()
{
    if (!PhotonNetwork.IsMasterClient) return;
    if (PhotonNetwork.CurrentRoom == null) return;
    if (PhotonNetwork.PlayerList.Length < 2) return;

    // 규칙: 마스터=백, 나머지 첫 번째=흑 (원하면 네 규칙으로 바꿔도 됨)
    int white = PhotonNetwork.MasterClient.ActorNumber;
    int black = PhotonNetwork.PlayerListOthers[0].ActorNumber;

    var ht = new PhotonHashtable
    {
        { "whiteActor", white },
        { "blackActor", black }
    };
    PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
    Debug.Log($"[AssignColors] whiteActor={white}, blackActor={black}");
}



    public float tileSize = 0.66f;                 // �ڵ� ���� ������ �⺻��
    public Vector2 boardOrigin = Vector2.zero;     // 0,0 ĭ(=a1)�� ���� ��ǥ(����)
    public float boardWorldWidth = 0f;
    public float boardWorldHeight = 0f;
    public override void OnRoomPropertiesUpdate(PhotonHashtable changedProps)
{
    var rp = PhotonNetwork.CurrentRoom?.CustomProperties;
    if (rp == null) return;

    if (rp.TryGetValue("whiteActor", out var wObj) && wObj is int w) whiteActor = w;
    if (rp.TryGetValue("blackActor", out var bObj) && bObj is int b) blackActor = b;

    if (whiteActor != -1 && blackActor != -1)
    {
        // 내 색 확정
        myPlayerColor = (PhotonNetwork.LocalPlayer.ActorNumber == whiteActor) ? "white" : "black";

        // TimeManager와 동일한 출처로 만들어주기
        PlayerPrefs.SetString("MyColor", myPlayerColor);
        PlayerPrefs.Save();

        Debug.Log($"[ColorReady] local={PhotonNetwork.LocalPlayer.ActorNumber}, myColor={myPlayerColor}, whiteActor={whiteActor}, blackActor={blackActor}");
    }
}




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
        TryAssignColors(); 

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

    public GameObject Create(string name, int x, int y)
    {
        Vector3 spawnPos = new Vector3(0f, 0f, 0);
        GameObject obj = PhotonNetwork.Instantiate("Pieces/MultiChesspiece", spawnPos, Quaternion.identity);
        var sr = obj.GetComponent<SpriteRenderer>();

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

    public string GetMyPlayerColor() {
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
        
        // ������ �ƴϸ� �ƹ��͵� ���� ���� (����)
        if (!PhotonNetwork.IsMasterClient) return;

        // --- ���⿡�� ������ ���޹��� ������ �̵��� ��ȿ���� ������ �� �ֽ��ϴ� ---
        // ��: ���� �´���, ��Ģ�� �´��� �� (������ ����)
        // ---------------------------------------------------------------------

        // ĳ���� ��û�� ���
        if (isCastle)
        {
            // ĳ���� ���� RPC�� ��� Ŭ���̾�Ʈ���� ����
            photonView.RPC("RPC_ExecuteCastling", RpcTarget.All, attackerID, targetX);
        }
        else // �Ϲ� �̵�/���� ��û�� ���
        {
            // 1. ������ ���� ������ �ı� RPC ����
            if (capturedID != -1)
            {
                PhotonView.Find(capturedID)?.RPC("DestroySelf", RpcTarget.AllBuffered);
            }

            // 2. �����ϴ� ���� �̵���Ű�� RPC ����
            PhotonView.Find(attackerID)?.RPC("MoveTo", RpcTarget.AllBuffered, targetX, targetY);
        }

        // ���� ���� ù �̵� ���¸� ������Ʈ�ϴ� RPC ȣ�� ����
        PhotonView.Find(attackerID)?.RPC("RPC_UpdateMovedStatus", RpcTarget.All);

        // 4. ��� ó���� �������Ƿ�, ���� ������ �ѱ�
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
        kingCm.SetXBoard(kingTargetX); // 1. ���� X��ǥ�� ���� �ٲٰ�
                                       // kingCm.SetYBoard(y); // y�� �״������, ��Ȯ���� ���� ���൵ �����ϴ�.
        kingCm.SetCoords();           // 2. �ٲ� ���� ��ǥ�� ���� ���� ��ġ�� �ű�� �Ѵ�.

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
            rookCm.SetXBoard(rookTargetX); // 1. ���� X��ǥ�� ���� �ٲٰ�
            rookCm.SetCoords();            // 2. ���� ��ġ�� �ű�� �Ѵ�.

            SetPosition(rookObj);
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