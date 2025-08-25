using Photon.Pun;
using UnityEngine;

public class LoginUI : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject loginPanel;   // 로그인 입력/버튼 패널
    [SerializeField] GameObject mainMenuPanel; // 방 만들기/입장 메뉴 패널

    void ProceedAfterLogin()
    {
        if (loginPanel) loginPanel.SetActive(false);
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        Debug.Log("[LoginUI] proceed (InLobby=" + PhotonNetwork.InLobby + ")");
    }

    // 버튼 OnClick에 연결
    public void OnClickLogin()
    {
        // 이미 로비면 바로 다음 단계
        if (PhotonNetwork.InLobby) { ProceedAfterLogin(); return; }

        // 마스터에 붙어 있으면 로비 참가만
        if (PhotonNetwork.IsConnected) { PhotonNetwork.JoinLobby(); return; }

        // 아직 미연결이면 접속 시도
        PhotonNetwork.ConnectUsingSettings();
    }

    // 연결되면 로비 참가 보장
    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }

    // 로비 들어오면 성공 처리
    public override void OnJoinedLobby()
    {
        ProceedAfterLogin();
    }
}
