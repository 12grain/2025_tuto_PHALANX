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
            Debug.Log("<color=yellow>Ŭ�� �м� ����:</color> " + Input.mousePosition);

            // 1. UI Ŭ�� Ȯ��
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;
            List<RaycastResult> uiResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, uiResults);

            if (uiResults.Count > 0)
            {
                Debug.LogWarning("Ŭ���� UI�� �������ϴ�! ���� ���� �ִ� UI: " + uiResults[0].gameObject.name);
                Debug.Log("=====================================");
                return;
            }

            // 2. 2D ���� Ŭ�� Ȯ�� (��� �浹ü)
            if (Camera.main == null)
            {
                Debug.LogError("����: ���� 'MainCamera' �±װ� ���� ī�޶� �����ϴ�.");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Scene �信�� ����׿� ������ 10�ʰ� ������
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.magenta, 10.0f);

            // ������ ����ϴ� ��� 2D �ݶ��̴��� ������
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            if (hits.Length > 0)
            {
                Debug.Log($"<color=cyan>�� {hits.Length}���� 2D �ݶ��̴��� �����߽��ϴ�. (ī�޶� ����� �������):</color>");
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit2D hit = hits[i];
                    string triggerStatus = hit.collider.isTrigger ? " (Ʈ����)" : " (�Ϲ� �ݶ��̴�)";
                    Debug.Log($"  #{i + 1}: �̸�='{hit.collider.gameObject.name}', Z ��ġ='{hit.collider.transform.position.z}'" + triggerStatus);
                }
            }
            else
            {
                Debug.Log("<color=grey>������ 2D �ݶ��̴��� �����ϴ�.</color>");
            }
            Debug.Log("=====================================");
        }
    }
}