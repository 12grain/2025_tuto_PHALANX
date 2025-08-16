using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PromotionManager : MonoBehaviourPunCallbacks
{
    public static PromotionManager Instance; // 다른 스크립트에서 쉽게 접근하기 위한 static instance

    [Header("UI Elements")]
    public GameObject promotionPanel;
    public Button queenButton, rookButton, bishopButton, knightButton;

    private MultiGame gameController;

    // 프로모션을 위해 임시로 저장해야 할 정보들
    private MultiChessMan pawnToPromote;
    private int targetX;
    private int targetY;
    private bool isAttack;
    private int capturedID;

    void Awake()
    {
        Instance = this; // 자기 자신을 static instance에 등록
        promotionPanel.SetActive(false);
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<MultiGame>();
    }

    void Start()
    {
        // 버튼을 누르면 PromoteAs 함수가 호출되도록 설정 (이 부분은 동일)
        queenButton.onClick.AddListener(() => PromoteAs("queen"));
        rookButton.onClick.AddListener(() => PromoteAs("rook"));
        bishopButton.onClick.AddListener(() => PromoteAs("bishop"));
        knightButton.onClick.AddListener(() => PromoteAs("knight"));
    }

    // 이 함수는 더 이상 RPC가 아닙니다. MultiChessMan이 직접 로컬에서 호출합니다.
    public void ShowPromotionUI(MultiChessMan pawn, int tX, int tY, bool attack, int capID)
    {
        // 필요한 정보를 모두 저장
        pawnToPromote = pawn;
        targetX = tX;
        targetY = tY;
        isAttack = attack;
        capturedID = capID;

        // UI를 보여줌
        promotionPanel.SetActive(true);

        // 모든 클라이언트의 상호작용을 막도록 방장에게 요청
        gameController.GetComponent<PhotonView>().RPC("RPC_SetInteractionState", RpcTarget.All, true);
    }

    // 버튼을 클릭했을 때 실행되는 함수
    void PromoteAs(string pieceType)
    {
        // UI를 즉시 숨김
        promotionPanel.SetActive(false);

        if (pawnToPromote == null) return;

        // 방장에게 "모든 준비가 끝났으니 프로모션을 실행해주세요" 라고 최종 요청을 보냄
        gameController.GetComponent<PhotonView>().RPC("RPC_ExecutePromotion", RpcTarget.MasterClient,
            pawnToPromote.photonView.ViewID, targetX, targetY, capturedID, pieceType);
    }
}