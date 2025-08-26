// Assets/Scripts/Net/MatchColorAssigner.cs
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchColorAssigner : MonoBehaviourPunCallbacks
{
    public override void OnJoinedRoom()               { TryAssign(); }
    public override void OnPlayerEnteredRoom(Player p){ TryAssign(); }

    private void TryAssign()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        // 이미 세팅되어 있으면 스킵
        if (room.CustomProperties != null &&
            room.CustomProperties.ContainsKey("whiteActor") &&
            room.CustomProperties.ContainsKey("blackActor")) return;

        if (PhotonNetwork.PlayerList.Length < 2) return;

        // 규칙: 마스터=백, 상대=흑 (원하면 네 규칙으로 바꿔도 됨)
        int white = PhotonNetwork.MasterClient.ActorNumber;
        int black = PhotonNetwork.PlayerListOthers[0].ActorNumber;

        var ht = new Hashtable { { "whiteActor", white }, { "blackActor", black } };
        room.SetCustomProperties(ht);

        Debug.Log($"[AssignColors] whiteActor={white}, blackActor={black}");
    }
}
