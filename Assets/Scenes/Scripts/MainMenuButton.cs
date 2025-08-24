    // MainMenuButton.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

[RequireComponent(typeof(Button))]
public class MainMenuButton : MonoBehaviour
{
    [SerializeField] string mainMenuSceneName = "LobbyScene"; // 네 메인메뉴 씬 이름으로 변경

    Button btn;
    bool leaving;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClickMainMenu);
         // 부모 CanvasGroup이 있으면 자식 클릭 막지 않도록
    var cg = GetComponentInParent<CanvasGroup>();
    if (cg) { cg.blocksRaycasts = true; cg.interactable = true; cg.alpha = 1f; }

    // 바로 위 배경 이미지가 있다면 Raycast 막지 않게
    var parentImg = transform.parent.GetComponent<UnityEngine.UI.Image>();
    if (parentImg) parentImg.raycastTarget = false;

    // 내 버튼/이미지는 정상 클릭 가능
    var img = GetComponent<UnityEngine.UI.Image>();
    if (img) img.raycastTarget = true;
    }

    void OnClickMainMenu()
    {
        if (leaving) return;
        leaving = true;
        btn.interactable = false;

        // 혹시 게임 중 커서 숨겼다면 되살리기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;

        StartCoroutine(LeaveAndLoad());
    }

    System.Collections.IEnumerator LeaveAndLoad()
    {
        // 방 안에 있으면 먼저 나가고, 완전히 나갈 때까지 대기
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom) yield return null;
        }

        SceneManager.LoadScene("LobbyScene");
    }
    
}




