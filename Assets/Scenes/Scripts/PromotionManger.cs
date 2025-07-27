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


    // 프로모션할 폰(Chessman) 참조
    private Chessman pawnToPromote;
    public GameObject chesspiecePrefab;

    void Awake()
    {
        // 처음엔 숨겨두기
        promotionPanel.SetActive(false);
    }
    void Start()
    {
        // 버튼에 리스너 연결
        queenButton.onClick.AddListener(() => PromoteAs("queen"));
        rookButton.onClick.AddListener(() => PromoteAs("rook"));
        bishopButton.onClick.AddListener(() => PromoteAs("bishop"));
        knightButton.onClick.AddListener(() => PromoteAs("knight"));
    }

    /// <summary>
    /// Chessman 쪽에서 프로모션이 필요할 때 호출하세요.
    /// </summary>
    public void ShowPromotionUI(Chessman pawn)
    {
        pawnToPromote = pawn;
        promotionPanel.SetActive(true);
    }

    /// <summary>
    /// 선택된 pieceType 으로 pawnToPromote를 교체.
    /// </summary>
    void PromoteAs(string pieceType)
    {
        // 1) 기존 폰 제거
        int x = pawnToPromote.GetXBoard();
        int y = pawnToPromote.GetYBoard();
        Vector3 worldPos = pawnToPromote.transform.position;
        Destroy(pawnToPromote.gameObject);
        var game = GameObject.FindGameObjectWithTag("GameController")
                             .GetComponent<Game>();
        game.SetPositionEmpty(x, y);

        // 2) Chesspiece 프리팹 인스턴트, 이름 세팅
        GameObject newPiece = Instantiate(chesspiecePrefab, worldPos, Quaternion.identity);
        // 이름을 바꾸면 Activate() 로직에서 sprite/player 분기 처리됨
        newPiece.name = $"{pawnToPromote.GetPlayer()}_{pieceType}";

        // 3) 좌표 초기화 & Activate 호출
        var cm = newPiece.GetComponent<Chessman>();
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();

        // 4) 보드에 등록
        game.SetPosition(newPiece);

        // 5) UI 닫고 턴 넘기기
        promotionPanel.SetActive(false);
        game.NextTurn();
        Debug.Log($"Promote called! prefab={chesspiecePrefab}, pawn={pawnToPromote}");
    }
}
