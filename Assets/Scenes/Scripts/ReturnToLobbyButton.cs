using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Button))]
public class ReturnToLobbyButton : MonoBehaviour
{
    [SerializeField] string lobbySceneName = "LobbyScene";
    [SerializeField] float leaveRoomTimeout = 6f;
    [SerializeField] float connectTimeout   = 10f;
    [SerializeField] float joinLobbyTimeout = 6f;

    Button btn;
    bool inProgress;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        if (btn) btn.onClick.RemoveListener(OnClick);
    }

    void OnClick()
    {
        if (inProgress) return;
        inProgress = true;
        btn.interactable = false;
        AppFlow.CameFromMatch = true;

        // 혹시 멈춤/커서 잠금이 있으면 복구
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 게임씬의 재시작 로직이 돌지 않게 차단(있을 경우)
        var mg = FindObjectOfType<MultiGame>();
        if (mg) mg.enabled = false;

        StartCoroutine(ReturnFlow());
    }

    System.Collections.IEnumerator ReturnFlow()
    {
        Debug.Log($"[Return] start state={PhotonNetwork.NetworkClientState}");

        // 1) 방에서 안전하게 나가기 (타임아웃 포함)
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            float t = 0f;
            while (PhotonNetwork.InRoom && t < leaveRoomTimeout)
            {
                t += Time.unscaledDeltaTime;   // 타임스케일 무관
                yield return null;
            }
            Debug.Log($"[Return] LeaveRoom done? {!PhotonNetwork.InRoom}, t={t:0.00}");
        }

        // 2) 게임서버→마스터서버 전환 대기 (타임아웃 포함)
        {
            float t = 0f;
            while ((PhotonNetwork.NetworkClientState == ClientState.DisconnectingFromGameServer ||
                    PhotonNetwork.NetworkClientState == ClientState.ConnectingToMasterServer) &&
                   t < 5f)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            Debug.Log($"[Return] transition done, state={PhotonNetwork.NetworkClientState}");
        }

        // 3) 마스터서버 연결 보장
        if (!PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        {
            Debug.Log("[Return] reconnecting to master...");
            PhotonNetwork.ConnectUsingSettings();
        }
        {
            float t = 0f;
            while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMaster && t < connectTimeout)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            Debug.Log($"[Return] ConnectedToMaster? {PhotonNetwork.NetworkClientState == ClientState.ConnectedToMaster}");
        }

        // 4) 로비 참가 보장
        if (!PhotonNetwork.InLobby)
        {
            Debug.Log("[Return] joining lobby...");
            PhotonNetwork.JoinLobby();
            float t = 0f;
            while (!PhotonNetwork.InLobby && t < joinLobbyTimeout)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        Debug.Log($"[Return] InLobby={PhotonNetwork.InLobby}, state={PhotonNetwork.NetworkClientState}");

        // 5) 로비 씬 로드 (로비 씬에서 추가 보정 스크립트가 있으면 더 안전)
        SceneManager.LoadScene(lobbySceneName);
    }
}
