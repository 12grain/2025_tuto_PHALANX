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

    void PromoteAs(string pieceType)
    {
        // 1) ���� �� ���� �迭���� ����
        int x = pawnToPromote.GetXBoard();
        int y = pawnToPromote.GetYBoard();
        gameController.SetPositionEmpty(x, y);

        // �׸��� ��Ʈ��ũ �ı�
        pawnToPromote.photonView.RPC("DestroySelf", RpcTarget.AllBuffered);

        // 2) ��Ʈ��ũ�� �� �⹰ ����
        Vector3 worldPos = new Vector3(x * 0.66f - 2.3f, y * 0.66f - 2.3f, -1f);
        GameObject newPiece = PhotonNetwork.Instantiate(
            "MultiChesspiece", worldPos, Quaternion.identity
        );
        // �̸� ����
        string ownerColor = pawnToPromote.GetPlayer();
        newPiece.name = ownerColor + "_" + pieceType;

        // 3) SetupSprite �� ���� �ʱ�ȭ
        //    (�� RPC �ȿ��� SetPosition �� �� �ֵ��� �̸� �ۼ��� �μ���)
        newPiece.GetComponent<PhotonView>()
                .RPC("SetupSprite",
                     RpcTarget.AllBuffered,
                     newPiece.name, x, y);

        // 4) UI �ݰ� �� ��ȯ
        promotionPanel.SetActive(false);
    }
}
