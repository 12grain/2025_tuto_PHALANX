// GameOverUITroubleshooter.cs  (Win/Lose 동시 지원)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameOverUITroubleshooter : MonoBehaviour
{
    [Header("Assign in Inspector (UI Panels under Canvas)")]
    public GameObject winPanelUI;
    public GameObject losePanelUI;

    [Header("Buttons (uGUI Button components)")]
    public Button winButton;
    public Button loseButton;

    [Header("Layout")]
    public Vector2 buttonSize = new Vector2(650f, 120f);
    public float buttonY = 100f; // 하단에서 얼마나 위로 올릴지(px)

    void Awake()
    {
        EnsureEventSystem();

        // 패널/버튼 각각 보정
        SetupPanel(winPanelUI, "WinPanelUI");
        SetupPanel(losePanelUI, "LosePanelUI");

        SetupButton(winButton,  "WinButton");
        SetupButton(loseButton, "LoseButton");
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log("[UI] EventSystem가 없어 자동 생성했습니다.");
        }
    }

    void SetupPanel(GameObject panel, string label)
    {
        if (!panel) { Debug.LogWarning($"[UI] {label} 미지정"); return; }

        var canvas = panel.GetComponentInParent<Canvas>();
        if (!canvas) { Debug.LogError($"[UI] {label} 이(가) Canvas 하위가 아닙니다."); return; }

        // Canvas 안전 세팅
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // 배경이 클릭 가로막지 않게
        var bgImg = panel.GetComponent<Image>();
        if (bgImg) bgImg.raycastTarget = false;

        // CanvasGroup 보정
        var cg = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
        cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;

        panel.SetActive(true);
    }

    void SetupButton(Button btn, string label)
    {
        if (!btn) { Debug.LogWarning($"[UI] {label} 미지정"); return; }
        if (!btn.GetComponentInParent<Canvas>()) { Debug.LogError($"[UI] {label} 이(가) Canvas 하위가 아닙니다."); return; }

        // 보이는 상태 & 레이캐스트 보장
        var img = btn.GetComponent<Image>() ?? btn.gameObject.AddComponent<Image>();
        img.color = new Color(1,1,1,1);    // 알파 255 (투명 방지)
        img.raycastTarget = true;

        // RectTransform 정렬(하단 중앙)
        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot    = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = buttonSize;
        rt.anchoredPosition = new Vector2(0f, buttonY);
        rt.localScale = Vector3.one;

        // 텍스트 중복 방지(배경에 글자 인쇄돼 있으면 꺼두기)
        var tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp) tmp.enabled = false;

        // 가림 방지: 최상단으로
        btn.transform.SetAsLastSibling();

        btn.interactable = true;
        btn.gameObject.SetActive(true);

        Debug.Log($"[UI] {label} 표시/정렬 완료 (Y={buttonY}, Size={buttonSize.x}x{buttonSize.y})");
    }
}
