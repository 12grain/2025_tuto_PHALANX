// GameOverUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameOverUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private GameObject root;      // Canvas 또는 최상위 패널
    [SerializeField] private Image resultImage;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Sprite victorySprite;
    [SerializeField] private Sprite defeatSprite;

    private void Awake()
    {
        if (root == null) root = gameObject;
        root.SetActive(false);
        mainMenuButton.onClick.AddListener(OnClickMainMenu);
    }

    public void Show(bool isWinner)
    {
        resultImage.sprite = isWinner ? victorySprite : defeatSprite;
        root.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // 여기서 보드 입력 스크립트 비활성화 하고 싶다면: BoardController.Instance.enabled = false; 등
    }

    private void OnClickMainMenu()
    {
        StartCoroutine(LeaveAndLoad());
    }

    private System.Collections.IEnumerator LeaveAndLoad()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom) yield return null;
        SceneManager.LoadScene("MainMenu"); // 너의 메인메뉴 씬 이름으로 변경
    }
}
