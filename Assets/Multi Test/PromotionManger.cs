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

    // PromotionManager.cs 의 PromoteAs 함수
    void PromoteAs(string pieceType)
    {
        // UI를 즉시 숨김
        promotionPanel.SetActive(false);

        if (pawnToPromote == null) return;

        // 방장에게 "이 폰을 이 타입으로 바꿔주세요" 라고 요청하는 RPC를 보냄
        gameController.GetComponent<PhotonView>().RPC("RequestPromotion", RpcTarget.MasterClient,
            pawnToPromote.photonView.ViewID, pieceType);
    }
}
