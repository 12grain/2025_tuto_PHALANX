using Photon.Pun;
using UnityEngine;

public class MultiMovePlate : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public bool isCastling = false;
    public GameObject controller; // GameController 참조
    GameObject reference = null;

    int matrixX;
    int matrixY;
    public bool attack = false;

    // 시각/배치 관련 설정
    [Header("Visual")]
    [Tooltip("칸 대비 차지 비율 (0.0 ~ 1.0)")]
    public float fillRatio = 0.9f;

    // Awake / Start
    void Awake()
    {
        if (controller == null)
            controller = GameObject.FindGameObjectWithTag("GameController");
    }

    void Start()
    {
        // NOTE: 이전 코드에서 비소유자에서 SpriteRenderer/Collider를 파괴하던 부분 제거.
        // MovePlate 는 모든 클라이언트에서 보여야 하므로 파괴하지 않습니다.

        if (attack)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(1f, 0f, 0f, 1f);
        }

        // 안전: 시각 속성 적용 (in case SetCoords/OnPhotonInstantiate didn't run yet)
        ApplyVisuals();
    }

    PhotonView pv;

    public void OnMouseUp()
    {
        pv = this.GetComponent<PhotonView>();
        var game = controller.GetComponent<MultiGame>();

        if (isCastling)
        {
            pv.RPC("HandleCastling", RpcTarget.All);
        }
        else
        {
            if (reference == null)
            {
                Debug.LogWarning("MovePlate clicked but reference is null");
                return;
            }

            int attackerID = reference.GetComponent<PhotonView>().ViewID;

            // 캡처 대상이 있으면 DestroySelf RPC 호출
            if (attack)
            {
                GameObject cp = game.GetPosition(matrixX, matrixY);
                if (cp != null)
                {
                    int capturedID = cp.GetComponent<PhotonView>().ViewID;
                    PhotonView.Find(capturedID)
                              .RPC("DestroySelf", RpcTarget.AllBuffered);
                }
            }

            // 이동 RPC 호출 (모든 클라이언트에서 동작)
            PhotonView.Find(attackerID)
                      .RPC("MoveTo", RpcTarget.AllBuffered, matrixX, matrixY);

            // 로컬 처리(플레이트 제거, 프로모션 분기 등)는 NormalMove RPC에서 처리
            NormalMove();
        }

        // OnMouseUp에서 턴 전환을 관리하는 구조면 남겨두고,
        // 만약 프로모션 브랜치에서 턴을 넘기지 않아야 하면 NormalMove() 내부에서 return 처리됨.
        controller.GetComponent<MultiGame>().CallNextTurn();
    }

    [PunRPC]
    private void HandleCastling()
    {
        int targetViewID = reference?.GetComponent<PhotonView>()?.ViewID ?? -1;
        reference = PhotonView.Find(targetViewID)?.gameObject;
        if (reference == null) return;

        MultiChessMan kingCm = reference.GetComponent<MultiChessMan>();
        MultiGame game = controller.GetComponent<MultiGame>();

        int oldKingX = kingCm.GetXBoard();
        int targetKingX = matrixX;
        int y = kingCm.GetYBoard();

        game.SetPositionEmpty(oldKingX, kingCm.GetYBoard());
        kingCm.SetXBoard(targetKingX);
        kingCm.SetCoords();
        game.SetPosition(reference);

        bool isKingSide = targetKingX > oldKingX;
        int rookOldX = isKingSide ? 7 : 0;
        int rookNewX = isKingSide ? targetKingX - 1 : targetKingX + 1;

        MultiChessMan[] allPieces = GameObject.FindObjectsOfType<MultiChessMan>();
        GameObject rookObj = null;

        foreach (MultiChessMan cm in allPieces)
        {
            if (cm.GetPlayer() == kingCm.GetPlayer()
                && cm.GetXBoard() == rookOldX
                && cm.GetYBoard() == y
                && cm.name.Contains("rook"))
            {
                rookObj = cm.gameObject;
                break;
            }
        }
        if (rookObj != null)
        {
            game.SetPositionEmpty(rookOldX, y);
            MultiChessMan rookCm = rookObj.GetComponent<MultiChessMan>();
            rookCm.SetXBoard(rookNewX);
            rookCm.SetCoords();
            game.SetPosition(rookObj);
        }

        game.CallNextTurn();
        kingCm.DestroyMovePlates();
        reference.GetComponent<MultiChessMan>().DisableCastling();
    }

    [PunRPC]
    public void NormalMove()
    {
        Debug.Log("Normal 들어옴");

        if (reference == null)
        {
            Debug.LogWarning("NormalMove: reference is null");
            return;
        }

        int PiecetargetViewID = reference.GetComponent<PhotonView>().ViewID;
        reference = PhotonView.Find(PiecetargetViewID)?.gameObject;

        if (reference == null)
        {
            Debug.LogWarning("NormalMove: reference lookup failed");
            return;
        }

        // Pawn 처리(이동 불가 플레이트 제거, 프로모션 등)
        if (reference.name.Contains("pawn"))
        {
            reference.GetComponent<MultiChessMan>().DisableDoubleMove();
            reference.GetComponent<MultiChessMan>().DestroyMovePlates();

            int promotionY = reference.GetComponent<MultiChessMan>().GetPlayer() == "white" ? 2 : 5;
            if (matrixY == promotionY)
            {
                int pawnID = reference.GetComponent<PhotonView>().ViewID;
                controller.GetComponent<PhotonView>()
                          .RPC("RPC_ShowPromotionUI",
                               reference.GetComponent<PhotonView>().Owner,
                               pawnID);
                return; // 프로모션의 경우 여기서 멈춤
            }
        }

        if (reference.name.Contains("king"))
        {
            reference.GetComponent<MultiChessMan>().DisableCastling();
        }


        // 일반 이동 후 플레이트 제거 (턴 전환은 호출하는 쪽에서 관리)
        reference.GetComponent<MultiChessMan>().DestroyMovePlates();
    }

    // SetCoords: matrix 좌표를 저장하고 시각·충돌 적용
    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
        ApplyVisuals();
    }

    // SetReference: 로컬에서 호출하여 레퍼런스 설정 -> 다른 클라이언트에 viewID를 전송해서 동기화
    public void SetReference(GameObject obj)
    {
        reference = obj;
        int viewID = obj.GetComponent<PhotonView>().ViewID;
        photonView.RPC("SyncReference", RpcTarget.OthersBuffered, viewID);
    }

    [PunRPC]
    public void SyncReference(int viewID)
    {
        GameObject target = PhotonView.Find(viewID)?.gameObject;
        if (target != null)
            reference = target;
        else
            Debug.LogWarning("MovePlate: reference 동기화 실패 (viewID: " + viewID + ")");
    }

    public GameObject GetReference() => reference;

    // IPunInstantiateMagicCallback
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = photonView.InstantiationData;
        if (instantiationData != null && instantiationData.Length >= 2)
        {
            int x = (int)instantiationData[0];
            int y = (int)instantiationData[1];
            matrixX = x;
            matrixY = y;
            // remote clients get coordinates from instantiation data -> apply visuals
            ApplyVisuals();
        }
    }

    // 시각/충돌을 실제로 적용하는 함수 (모든 클라이언트에서 동일하게 실행)
    private void ApplyVisuals()
    {
        // GameController 가 준비되지 않았으면 나중에 다시 호출될 수 있도록 안전하게 처리
        var gameObj = GameObject.FindGameObjectWithTag("GameController");
        if (gameObj == null) return;
        var game = gameObj.GetComponent<MultiGame>();
        if (game == null) return;

        float tile = game.tileSize;
        Vector2 origin = game.boardOrigin;

        // 월드 좌표: 칸 중심
        float worldX = origin.x + matrixX * tile;
        float worldY = origin.y + matrixY * tile;

        // z 값을 살짝 앞쪽으로 올려서 보이도록 (z는 렌더링보다 안전장치)
        transform.position = new Vector3(worldX, worldY, -0.4f);

        // SpriteRenderer 설정 (정렬 레이어 / order)
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "MovePlates";
            sr.sortingOrder = 3;

            // 스케일 조정: 칸 대비 fillRatio만큼의 폭을 차지
            if (sr.sprite != null && sr.sprite.bounds.size.x > 0.0001f)
            {
                float desiredWorldWidth = tile * Mathf.Clamp01(fillRatio);
                float originalSpriteWidth = sr.sprite.bounds.size.x; // units at scale=1
                float scale = desiredWorldWidth / originalSpriteWidth;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        // Collider 조정
        var bc = GetComponent<BoxCollider2D>();
        if (bc != null)
        {
            bc.size = new Vector2(tile * 0.95f, tile * 0.95f);
            bc.offset = Vector2.zero;
        }
    }
}
