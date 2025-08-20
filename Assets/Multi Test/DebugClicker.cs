using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DebugClickAnalyzer : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("=====================================");
            Debug.Log("<color=yellow>클릭 분석 시작:</color> " + Input.mousePosition);

            // 1. UI 클릭 확인
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;
            List<RaycastResult> uiResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, uiResults);

            if (uiResults.Count > 0)
            {
                Debug.LogWarning("클릭이 UI에 막혔습니다! 가장 위에 있는 UI: " + uiResults[0].gameObject.name);
                Debug.Log("=====================================");
                return;
            }

            // 2. 2D 물리 클릭 확인 (모든 충돌체)
            if (Camera.main == null)
            {
                Debug.LogError("오류: 씬에 'MainCamera' 태그가 붙은 카메라가 없습니다.");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Scene 뷰에서 디버그용 광선을 10초간 보여줌
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.magenta, 10.0f);

            // 광선이 통과하는 모든 2D 콜라이더를 가져옴
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            if (hits.Length > 0)
            {
                Debug.Log($"<color=cyan>총 {hits.Length}개의 2D 콜라이더를 감지했습니다. (카메라에 가까운 순서대로):</color>");
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit2D hit = hits[i];
                    string triggerStatus = hit.collider.isTrigger ? " (트리거)" : " (일반 콜라이더)";
                    Debug.Log($"  #{i + 1}: 이름='{hit.collider.gameObject.name}', Z 위치='{hit.collider.transform.position.z}'" + triggerStatus);
                }
            }
            else
            {
                Debug.Log("<color=grey>감지된 2D 콜라이더가 없습니다.</color>");
            }
            Debug.Log("=====================================");
        }
    }
}