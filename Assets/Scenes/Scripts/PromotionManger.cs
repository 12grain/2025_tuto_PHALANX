using UnityEngine;
using UnityEngine.UI;

public class PromotionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject promotionPanel;    // PromotionPanel GameObject
    public Button queenButton;
    public Button rookButton;
    public Button bishopButton;
    public Button knightButton;


    // ���θ���� ��(Chessman) ����
    private Chessman pawnToPromote;
    public GameObject chesspiecePrefab;

    void Awake()
    {
        // ó���� ���ܵα�
        promotionPanel.SetActive(false);
    }
    void Start()
    {
        // ��ư�� ������ ����
        queenButton.onClick.AddListener(() => PromoteAs("queen"));
        rookButton.onClick.AddListener(() => PromoteAs("rook"));
        bishopButton.onClick.AddListener(() => PromoteAs("bishop"));
        knightButton.onClick.AddListener(() => PromoteAs("knight"));
    }

    /// <summary>
    /// Chessman �ʿ��� ���θ���� �ʿ��� �� ȣ���ϼ���.
    /// </summary>
    public void ShowPromotionUI(Chessman pawn)
    {
        pawnToPromote = pawn;
        promotionPanel.SetActive(true);
    }

    /// <summary>
    /// ���õ� pieceType ���� pawnToPromote�� ��ü.
    /// </summary>
    void PromoteAs(string pieceType)
    {
        // 1) ���� �� ����
        int x = pawnToPromote.GetXBoard();
        int y = pawnToPromote.GetYBoard();
        Vector3 worldPos = pawnToPromote.transform.position;
        Destroy(pawnToPromote.gameObject);
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<Game>();
        game.SetPositionEmpty(x, y);

        // 2) Chesspiece ������ �ν���Ʈ, �̸� ����
        GameObject newPiece = Instantiate(chesspiecePrefab, worldPos, Quaternion.identity);
        // �̸��� �ٲٸ� Activate() �������� sprite/player �б� ó����
        newPiece.name = $"{pawnToPromote.GetPlayer()}_{pieceType}";

        // 3) ��ǥ �ʱ�ȭ & Activate ȣ��
        var cm = newPiece.GetComponent<Chessman>();
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();

        // 4) ���忡 ���
        game.SetPosition(newPiece);

        // 5) UI �ݰ� �� �ѱ��
        promotionPanel.SetActive(false);
        game.NextTurn();
        Debug.Log($"Promote called! prefab={chesspiecePrefab}, pawn={pawnToPromote}");
    }
}
