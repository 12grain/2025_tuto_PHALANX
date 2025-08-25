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

    private bool ended = false; // ★ 중복 호출/표시 방지

    void Start()
    {
        whiteRemainTime = whiteFullTime;
        blackRemainTime = blackFullTime;

        if (whiteClockHand != null)
            whiteStartRot = whiteClockHand.transform.rotation;

        if (blackClockHand != null)
            blackStartRot = blackClockHand.transform.rotation;

        if (WinPanel) WinPanel.SetActive(false);
        if (LosePanel) LosePanel.SetActive(false);
    }

    void Update()
    {
        if (ended) return; // ★ 이미 게임오버면 더 이상 처리 안 함

        // 현재 누구 턴인지
        if (multiGame != null)
            isWhiteTurn = (multiGame.GetCurrentPlayer() == "white");

        // 남은 시간 감산
        if (isWhiteTurn && whiteRemainTime > 0f)
        {
            whiteRemainTime -= Time.deltaTime;
        }
        else if (!isWhiteTurn && blackRemainTime > 0f)
        {
            blackRemainTime -= Time.deltaTime;
        }

        // ★ 타임오버 판정(감산 후, 한 번만)
        // 내 색은 PlayerPrefs 대신 multiGame 기준을 우선 사용
        string myColor = (multiGame != null ? multiGame.GetMyPlayerColor() : PlayerPrefs.GetString("MyColor"));
        if (!string.IsNullOrEmpty(myColor)) myColor = myColor.Trim().ToLowerInvariant();

        if (!ended && whiteRemainTime <= 0f)
        {
            whiteRemainTime = 0f;
            ended = true;

            bool amWhite = (myColor == "white");
            if (WinPanel)  WinPanel.SetActive(!amWhite); // 백 시간 종료 → 백 패배, 흑 승리
            if (LosePanel) LosePanel.SetActive(amWhite);
            return;
        }

        if (!ended && blackRemainTime <= 0f)
        {
            blackRemainTime = 0f;
            ended = true;

            bool amWhite = (myColor == "white");
            if (WinPanel)  WinPanel.SetActive(amWhite);  // 흑 시간 종료 → 흑 패배, 백 승리
            if (LosePanel) LosePanel.SetActive(!amWhite);
            return;
        }

        // 시계바늘 회전(연출)
        UpdateClockHand();

        // 필요 시 동기화 사용
        // photonView.RPC("SyncTime", RpcTarget.Others, whiteRemainTime, blackRemainTime, isWhiteTurn);
    }

    public void RequestChangeTurn()
    {
        photonView.RPC("ChangeTimeOwner", RpcTarget.All);
    }

    [PunRPC]
    public void ChangeTimeOwner()
    {
        isWhiteTurn = !isWhiteTurn;
        photonView.RPC("SyncTime", RpcTarget.All, whiteRemainTime, blackRemainTime, isWhiteTurn);
    }

    [PunRPC]
    public void SyncTime(float whiteTime, float blackTime, bool whiteTurn)
    {
        if (ended) return; // ★ 종료 후 동기화로 덮어쓰지 않음
        whiteRemainTime = whiteTime;
        blackRemainTime = blackTime;
        isWhiteTurn = whiteTurn;
    }

    void UpdateClockHand()
    {
        if (whiteClockHand != null)
        {
            float whitePercent = Mathf.Clamp01(whiteRemainTime / whiteFullTime); // ★ 안전 클램프
            float rotationAmount = -360f * (1 - whitePercent);
            whiteClockHand.transform.rotation = whiteStartRot * Quaternion.Euler(0, 0, rotationAmount);
        }

        if (blackClockHand != null)
        {
            float blackPercent = Mathf.Clamp01(blackRemainTime / blackFullTime); // ★ 안전 클램프
            float rotationAmount = -360f * (1 - blackPercent);
            blackClockHand.transform.rotation = blackStartRot * Quaternion.Euler(0, 0, rotationAmount);
        }
    }
}
