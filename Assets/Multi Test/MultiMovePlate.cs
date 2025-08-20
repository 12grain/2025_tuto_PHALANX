using UnityEngine;

// 아주 단순한 MonoBehaviour로 변경
public class MultiMovePlate : MonoBehaviour
{
    public bool isCastling = false; // 캐슬링 여부는 그대로 유지
    public GameObject controller;
    GameObject reference = null; // 나를 만든 체스 말

    int matrixX;
    int matrixY;
    public bool attack = false;
    public bool isFusion = false;

    [Header("Visual")]
    [Tooltip("칸 대비 채우기 비율 (0.0 ~ 1.0)")]
    public float fillRatio = 0.9f; // 90% 채우기 (이 값을 조절해 크기 변경)

    void OnMouseDown()
    {
        Debug.Log("<color=green>MOVEPLATE가 클릭되었습니다!</color> 이름: " + this.gameObject.name);
    }


    public void SetupVisuals(MultiGame game, int boardX, int boardY)
    {
        // 1. 위치 계산 (수정 없음)
        float worldX = game.boardOrigin.x + boardX * game.tileSize;
        float worldY = game.boardOrigin.y + boardY * game.tileSize;
        transform.position = new Vector3(worldX, worldY, -2.0f);

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // 2. 크기 계산 (수정 없음)
        if (sr.sprite != null)
        {
            sr.sortingLayerName = "MovePlates"; // "MovePlates" 층에 그리도록 설정
            sr.sortingOrder = 1; 

            float desiredWorldWidth = game.tileSize * Mathf.Clamp01(fillRatio);
            float originalSpriteWidth = sr.sprite.bounds.size.x;
            if (originalSpriteWidth > 0.0001f)
            {
                float scale = desiredWorldWidth / originalSpriteWidth;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        // 3. ★★★ 공격 색상 적용 로직 추가! ★★★
        if (attack)
        {
            // attack 변수가 true이면 SpriteRenderer의 색을 빨간색으로 변경
            sr.color = new Color(1.0f, 0f, 0f, 1.0f);
        }
        else if (isFusion)
        {
            sr.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        }
    }

    public void OnMouseUp()
    {
        if (reference != null)
        {
            // 나를 만든 체스말(reference)에게 이동 실행을 요청한다.
            // isCastling 정보도 함께 넘겨준다.
            reference.GetComponent<MultiChessMan>().ExecuteMove(matrixX, matrixY, attack, isCastling, isFusion);
        }
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    // 이제 이 함수는 단순히 변수만 설정한다.
    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
}