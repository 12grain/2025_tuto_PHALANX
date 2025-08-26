using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class LobbyAutoConnector : MonoBehaviourPunCallbacks
{
    void Start()
    {
        if (AppFlow.CameFromMatch)
            StartCoroutine(AutoConnectIfReturned());
        // 처음 실행(= CameFromMatch == false)일 땐 아무것도 안 함 → 로그인 UI 그대로.
    }

    IEnumerator AutoConnectIfReturned()
    {
        // 전환 중이면 잠깐 대기
        while (PhotonNetwork.NetworkClientState == ClientState.DisconnectingFromGameServer ||
               PhotonNetwork.NetworkClientState == ClientState.ConnectingToMasterServer)
            yield return null;

        if (!PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMaster)
                yield return null;
        }

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            while (!PhotonNetwork.InLobby) yield return null;
        }

        AppFlow.CameFromMatch = false; // ✅ 플래그 클리어
        Debug.Log("[Lobby] Auto reconnected & joined lobby after match");
        // 필요하면 여기서 로그인 패널 숨기고 메인메뉴 패널을 켠다.
        // e.g. FindObjectOfType<YourLobbyUI>()?.ShowMainMenu();
    }
}
