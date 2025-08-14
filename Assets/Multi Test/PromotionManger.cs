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
        Debug.Log("���θ�ǽ���");
        var pawnObj = PhotonView.Find(pawnViewID).gameObject;
        if (pawnObj != null)
            pawnToPromote = pawnObj.GetComponent<MultiChessMan>();
        promotionPanel.SetActive(true);
        Debug.Log("�ǳڼ�ȯ");
    }

    // PromotionManager.cs �� PromoteAs �Լ�
    void PromoteAs(string pieceType)
    {
        // UI�� ��� ����
        promotionPanel.SetActive(false);

        if (pawnToPromote == null) return;

        // ���忡�� "�� ���� �� Ÿ������ �ٲ��ּ���" ��� ��û�ϴ� RPC�� ����
        gameController.GetComponent<PhotonView>().RPC("RequestPromotion", RpcTarget.MasterClient,
            pawnToPromote.photonView.ViewID, pieceType);
    }
}
