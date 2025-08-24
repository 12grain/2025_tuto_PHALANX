// EndGameBroadcaster.cs
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class EndGameBroadcaster : MonoBehaviourPunCallbacks
{
    public static EndGameBroadcaster Instance { get; private set; }
    private bool ended = false;
    private GameOverUI ui;

    private void Awake()
    {
        Instance = this;
        ui = FindObjectOfType<GameOverUI>(true);
    }

    // 체크메이트 등 "승자"가 확정됐을 때 호출
    public void ReportWinByRule(int winnerActorNumber)
    {
        if (ended) return;
        ended = true;
        photonView.RPC(nameof(RpcEndGame), RpcTarget.All, winnerActorNumber);
    }

    // 내 타이머가 0초가 되어 "내가 패배"했을 때 내 클라에서 호출
    public void ReportTimeoutLoss()
    {
        if (ended) return;
        ended = true;

        // 1:1 체스이므로 상대는 PlayerListOthers[0]
        int opponent = PhotonNetwork.PlayerListOthers[0].ActorNumber;
        photonView.RPC(nameof(RpcEndGame), RpcTarget.All, opponent);
    }

    [PunRPC]
    private void RpcEndGame(int winnerActorNumber)
    {
        ended = true;

        if (ui == null) ui = FindObjectOfType<GameOverUI>(true);

        bool iAmWinner = PhotonNetwork.LocalPlayer.ActorNumber == winnerActorNumber;
        ui.Show(iAmWinner);
    }
}
