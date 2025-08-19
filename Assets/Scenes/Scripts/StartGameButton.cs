using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StartGameButton : MonoBehaviour
{
    public Image fadePanel;           // 검정색 패널 연결
    public float fadeDuration = 0.2f; // 암막 등장/사라짐 시간 (초)

    public void StartGame()
    {
        StartCoroutine(FadeAndLoadScene());
    }

    IEnumerator FadeAndLoadScene()
    {
        float timer = 0f;
        Color color = fadePanel.color;

        // ✅ 암막 등장 (투명 → 불투명)
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadePanel.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // ✅ 잠깐 유지 (선택: 0.1초)
        yield return new WaitForSeconds(0.1f);

        // ✅ 암막 사라짐 (불투명 → 투명)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (timer / fadeDuration));
            fadePanel.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // ✅ 씬 전환
        // SceneManager.LoadScene("MainMenu");
        SceneManager.LoadScene("LobbyScene");
    }
}
