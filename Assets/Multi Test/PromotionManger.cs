using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PromotionManager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public GameObject promotionPanel;
    public Button queenButton, rookButton, bishopButton, knightButton;
    public GameObject chesspiecePrefab;

    private MultiGame gameController;
    private MultiChessMan pawnToPromote;

    void Awake()
    {
        promotionPanel.SetActive(false);
        gameController = GameObject.FindGameObjectWithTag("GameController")
                                 .GetComponent<MultiGame>();
    }

    void Start()
    {
        queenButton.onClick.AddListener(() => PromoteAs("queen"));
        rookButton.onClick.AddListener(() => PromoteAs("rook"));
        bishopButton.onClick.AddListener(() => PromoteAs("bishop"));
        knightButton.onClick.AddListener(() => PromoteAs("knight"));
    }


    [PunRPC]
    public void RPC_ShowPromotionUI(int pawnViewID)
    {
        Debug.Log("프로모션시작");
        var pawnObj = PhotonView.Find(pawnViewID).gameObject;
        if (pawnObj != null)
            pawnToPromote = pawnObj.GetComponent<MultiChessMan>();
        promotionPanel.SetActive(true);
        Debug.Log("판넬소환");
    }

    void PromoteAs(string pieceType)
    {
        // 1) 기존 폰 보드 배열에서 제거
        int x = pawnToPromote.GetXBoard();
        int y = pawnToPromote.GetYBoard();
        gameController.SetPositionEmpty(x, y);

        // 그리고 네트워크 파괴
        pawnToPromote.photonView.RPC("DestroySelf", RpcTarget.AllBuffered);

        // 2) 네트워크로 새 기물 생성
        Vector3 worldPos = new Vector3(x * 0.66f - 2.3f, y * 0.66f - 2.3f, -1f);
        GameObject newPiece = PhotonNetwork.Instantiate(
            "MultiChesspiece", worldPos, Quaternion.identity
        );
        // 이름 세팅
        string ownerColor = pawnToPromote.GetPlayer();
        newPiece.name = ownerColor + "_" + pieceType;

        // 3) SetupSprite 로 로직 초기화
        //    (이 RPC 안에서 SetPosition 도 해 주도록 미리 작성해 두세요)
        newPiece.GetComponent<PhotonView>()
                .RPC("SetupSprite",
                     RpcTarget.AllBuffered,
                     newPiece.name, x, y);

        // 4) UI 닫고 턴 전환
        promotionPanel.SetActive(false);
    }
}
