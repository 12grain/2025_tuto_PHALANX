using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class TimeManager : MonoBehaviourPun
{
    

    public float whiteFullTime = 300f;
    public float blackFullTime = 300f;

    public float whiteRemainTime;
    public float blackRemainTime;

    public GameObject whiteClockHand;
    public GameObject blackClockHand;
    public GameObject Clock;

    public GameObject WinPanel;
    public GameObject LosePanel;

   // public TextMeshProUGUI whiteText;
   // public TextMeshProUGUI blackText;

    public bool isWhiteTurn = true;

    private Quaternion whiteStartRot;
    private Quaternion blackStartRot;

    public MultiGame multiGame;


    void Start()
    {
        whiteRemainTime = whiteFullTime;
        blackRemainTime = blackFullTime;

        if (whiteClockHand != null)
            whiteStartRot = whiteClockHand.transform.rotation;

        if (blackClockHand != null)
            blackStartRot = blackClockHand.transform.rotation;

        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
    }

    void Update()
    {
        // ������ Ŭ���̾�Ʈ�� �ð� ���
        // if (!PhotonNetwork.IsMasterClient) return;

        if (whiteRemainTime < 0f)
        {
            if (PlayerPrefs.GetString("MyColor") == "white")
            {
                LosePanel.SetActive(true);
            }
            else
            {
                WinPanel.SetActive(true);
            }

        }

        if (blackRemainTime < 0f)
        {
            if (PlayerPrefs.GetString("MyColor") == "black")
            {
                LosePanel.SetActive(true);
            }
            else
            {
                WinPanel.SetActive(true);
            }


        }

        if (multiGame.GetCurrentPlayer() == "white")
        { isWhiteTurn = true; }
        else { isWhiteTurn = false; }

        if (isWhiteTurn && whiteRemainTime > 0f)
        {
            whiteRemainTime -= Time.deltaTime;
            //photonView.RPC("SyncTime", RpcTarget.Others, whiteRemainTime, blackRemainTime, isWhiteTurn);
        }
        else if (!isWhiteTurn && blackRemainTime > 0f)
        {
            blackRemainTime -= Time.deltaTime;
            //photonView.RPC("SyncTime", RpcTarget.Others, whiteRemainTime, blackRemainTime, isWhiteTurn);
        }
        

        // ����: �ð� �ٴ� ȸ�� (���û���)
        UpdateClockHand();

        //whiteText.text = "White : " + whiteRemainTime;
       // blackText.text = "black : " + blackRemainTime;


        photonView.RPC("SyncTime", RpcTarget.Others, whiteRemainTime, blackRemainTime, isWhiteTurn);



    }
    public void RequestChangeTurn()
    {
        photonView.RPC("ChangeTimeOwner", RpcTarget.All);
    }

    [PunRPC]
    public void ChangeTimeOwner()
    {
        //if (!PhotonNetwork.IsMasterClient) return; // �����͸� ���� ����
        isWhiteTurn = !isWhiteTurn;

        // �� ���� �� �ð��� ����ȭ
        photonView.RPC("SyncTime", RpcTarget.All, whiteRemainTime, blackRemainTime, isWhiteTurn);
    }


    [PunRPC]
    public void SyncTime(float whiteTime, float blackTime, bool whiteTurn)
    {
        whiteRemainTime = whiteTime;
        blackRemainTime = blackTime;
        isWhiteTurn = whiteTurn;
    }

    void UpdateClockHand()
    {
        if (whiteClockHand != null)
        {
            float whitePercent = whiteRemainTime / whiteFullTime;
            float rotationAmount = -360f * (1 - whitePercent); // �� ������ ����
            whiteClockHand.transform.rotation = whiteStartRot * Quaternion.Euler(0, 0, rotationAmount);
        }

        if (blackClockHand != null)
        {
            float blackPercent = blackRemainTime / blackFullTime;
            float rotationAmount = -360f * (1 - blackPercent); // �� ������ ����
            blackClockHand.transform.rotation = blackStartRot * Quaternion.Euler(0, 0, rotationAmount);
        }
    }
}

