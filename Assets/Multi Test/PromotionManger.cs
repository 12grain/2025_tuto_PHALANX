using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PromotionManager : MonoBehaviourPunCallbacks
{
    public static PromotionManager Instance; // �ٸ� ��ũ��Ʈ���� ���� �����ϱ� ���� static instance

    [Header("UI Elements")]
    public GameObject promotionPanel;
    public Button queenButton, rookButton, bishopButton, knightButton;

    private MultiGame gameController;

    // ���θ���� ���� �ӽ÷� �����ؾ� �� ������
    private MultiChessMan pawnToPromote;
    private int targetX;
    private int targetY;
    private bool isAttack;
    private int capturedID;

    void Awake()
    {
        Instance = this; // �ڱ� �ڽ��� static instance�� ���
        promotionPanel.SetActive(false);
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<MultiGame>();
    }

    void Start()
    {
        // ��ư�� ������ PromoteAs �Լ��� ȣ��ǵ��� ���� (�� �κ��� ����)
        queenButton.onClick.AddListener(() => PromoteAs("queen"));
        rookButton.onClick.AddListener(() => PromoteAs("rook"));
        bishopButton.onClick.AddListener(() => PromoteAs("bishop"));
        knightButton.onClick.AddListener(() => PromoteAs("knight"));
    }

    // �� �Լ��� �� �̻� RPC�� �ƴմϴ�. MultiChessMan�� ���� ���ÿ��� ȣ���մϴ�.
    public void ShowPromotionUI(MultiChessMan pawn, int tX, int tY, bool attack, int capID)
    {
        // �ʿ��� ������ ��� ����
        pawnToPromote = pawn;
        targetX = tX;
        targetY = tY;
        isAttack = attack;
        capturedID = capID;

        // UI�� ������
        promotionPanel.SetActive(true);

        // ��� Ŭ���̾�Ʈ�� ��ȣ�ۿ��� ������ ���忡�� ��û
        gameController.GetComponent<PhotonView>().RPC("RPC_SetInteractionState", RpcTarget.All, true);
    }

    // ��ư�� Ŭ������ �� ����Ǵ� �Լ�
    void PromoteAs(string pieceType)
    {
        // UI�� ��� ����
        promotionPanel.SetActive(false);

        if (pawnToPromote == null) return;

        // ���忡�� "��� �غ� �������� ���θ���� �������ּ���" ��� ���� ��û�� ����
        gameController.GetComponent<PhotonView>().RPC("RPC_ExecutePromotion", RpcTarget.MasterClient,
            pawnToPromote.photonView.ViewID, targetX, targetY, capturedID, pieceType);
    }
}