using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameOverUITroubleshooter : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject winPanelUI;           // Canvas 밑의 UI 패널이어야 함 (Image 포함 가능)
    public Button mainMenuButton;           // 진짜 uGUI 버튼 (Image + Button)

    void Awake()
    {
        // 1) EventSystem 보장
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log("[UI] EventSystem가 없어 자동 생성했습니다.");
        }

        // 2) WinPanel이 Canvas 밑에 있는지 확인
        if (winPanelUI != null)
        {
            var canvas = winPanelUI.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UI] WinPanel이 Canvas 하위가 아닙니다. Canvas( Screen Space - Overlay ) 밑으로 옮기세요.");
            }
            else
            {
                // Canvas 안전 설정
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                var cg = winPanelUI.GetComponent<CanvasGroup>();
                if (cg == null) cg = winPanelUI.AddComponent<CanvasGroup>();
                cg.alpha = 1f; cg.blocksRaycasts = false;  // 배경이 클릭을 막지 않도록
                winPanelUI.SetActive(true);
            }
        }

        // 3) 버튼 강제 표시 & 정렬
        if (mainMenuButton != null)
        {
            if (mainMenuButton.GetComponentInParent<Canvas>() == null)
            {
                Debug.LogError("[UI] 버튼이 Canvas 하위가 아닙니다. 버튼을 Canvas > WinPanel 밑으로 옮기세요.");
            }

            var img = mainMenuButton.GetComponent<Image>();
            if (img == null) img = mainMenuButton.gameObject.AddComponent<Image>();
            // 보이게 (투명 방지)
            img.color = new Color(1, 1, 1, 1);

            // 버튼 위치/크기 강제 중앙 정렬
            var rt = mainMenuButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot    = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 100f); // 화면 하단 위 100px
            rt.sizeDelta = new Vector2(650f, 120f);
            rt.localScale = Vector3.one;

            // 텍스트 중복이면 자식 TMP 텍스트 끔 (배경에 글자 이미 있으면)
            var tmp = mainMenuButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.enabled = false;

            mainMenuButton.gameObject.SetActive(true);
            Debug.Log("[UI] 버튼을 강제로 표시/정렬했습니다. 화면 하단 중앙을 확인하세요.");
        }
        else
        {
            Debug.LogError("[UI] mainMenuButton이 비어 있습니다. 실제 uGUI Button을 드래그해 연결하세요.");
        }
    }
}
