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
    private bool ended = false;  // 중복 처리 방지
    
    public MultiGame multiGame;
    private bool AmWhite()
{
    // MultiGame이 정해둔 값을 그대로 사용 (단일 출처)
    return multiGame != null && multiGame.GetMyPlayerColor() == "white";
}



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
        if (ended) return;


        if (!ended && whiteRemainTime <= 0f)
{
    ended = true;
    whiteRemainTime = 0f;

    bool amWhite = AmWhite();       // 백 시간 종료 → 백 패배 / 흑 승리
    WinPanel.SetActive(!amWhite);
    LosePanel.SetActive(amWhite);
    return;
}

if (!ended && blackRemainTime <= 0f)
{
    ended = true;
    blackRemainTime = 0f;

    bool amWhite = AmWhite();       // 흑 시간 종료 → 흑 패배 / 백 승리
    WinPanel.SetActive(amWhite);
    LosePanel.SetActive(!amWhite);
    return;
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

