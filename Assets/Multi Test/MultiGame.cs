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

    // Positions and team for each chesspiece
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private string myPlayerColor;
    private string currentPlayer = "white";

    private bool gameOver = false;
    private PhotonView pv;

    private bool turnChanged = false;


    void Awake()
    {
        myPlayerColor = PlayerPrefs.GetString("MyColor"); 
        Debug.Log("�� �÷��̾� ����: " + myPlayerColor);
    }
    void Start()
    {
          

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


    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = PhotonNetwork.Instantiate("MultiChesspiece", new Vector3(0, 0, 1), Quaternion.identity);
        MultiChessMan cm = obj.GetComponent<MultiChessMan>();
        //cm.name = name;
        //cm.SetXBoard(x);
        //cm.SetYBoard(y);
        //cm.Activate();
     
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

    //NextTurn()�� ���簡 ���� �����̸� �濡��, ���� �����̸� �鿡�� �ѱ�� �˰����� �����մϴ�.
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
      
    }

 
   

    public void CallNextTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("NextTurn", RpcTarget.AllBuffered);
        }
    }

    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject mp in movePlates)
        {
            Destroy(mp);
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