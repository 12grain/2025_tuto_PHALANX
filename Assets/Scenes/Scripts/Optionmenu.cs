// OptionsMenu.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class OptionsMenu : MonoBehaviour
{
    [Header("Roots")]
    [Tooltip("옵션 전체를 감싸는 루트(Backdrop + Panel). 처음엔 Inactive 권장.")]
    public GameObject optionsRoot;        // e.g., OptionsRoot

    [Header("Sliders")]
    public Slider masterSlider;           // Row_Master/Slider
    public Slider bgmSlider;              // Row_BGM/Slider
    public Slider sfxSlider;              // Row_SFX/Slider

    [Header("Audio Mixer (권장)")]
    public AudioMixer audioMixer;         // MasterMixer Drag
    [Tooltip("Master 그룹 Volume 파라미터명 (Expose된 이름)")]
    public string masterParam = "MASTER_VOL";
    [Tooltip("BGM(=Music) 그룹 Volume 파라미터명 (Expose된 이름)")]
    public string bgmParam    = "MUSIC.VOL";  // ← 네 프로젝트에 맞춰 기본값 설정
    [Tooltip("SFX 그룹 Volume 파라미터명 (Expose된 이름)")]
    public string sfxParam    = "SFX_VOL";

    [Header("Optional (Mixer 미사용시 보조)")]
    [Tooltip("BGM 재생 AudioSource (Mixer 연결이 어려울 때 볼륨 보조용)")]
    public AudioSource bgmSource;

    [Header("Defaults")]
    [Range(0,1)] public float defaultMaster = 0.8f;
    [Range(0,1)] public float defaultBgm    = 0.6f;
    [Range(0,1)] public float defaultSfx    = 0.7f;

    [Header("Quit to Title (선택)")]
    [Tooltip("타이틀로 나갈 때 로드할 씬 이름")]
    public string mainMenuSceneName = "MainMenu";

    // PlayerPrefs Keys
    const string KEY_MASTER = "OPT_MASTER";
    const string KEY_BGM    = "OPT_BGM";
    const string KEY_SFX    = "OPT_SFX";

    void Awake()
    {
        // 저장값 로드 → 슬라이더 값 반영
        float m = PlayerPrefs.GetFloat(KEY_MASTER, defaultMaster);
        float b = PlayerPrefs.GetFloat(KEY_BGM,    defaultBgm);
        float s = PlayerPrefs.GetFloat(KEY_SFX,    defaultSfx);

        if (masterSlider) masterSlider.value = m;
        if (bgmSlider)    bgmSlider.value    = b;
        if (sfxSlider)    sfxSlider.value    = s;

        // 즉시 적용
        ApplyMaster(m);
        ApplyBgm(b);
        ApplySfx(s);

        // 리스너 연결
        if (masterSlider) masterSlider.onValueChanged.AddListener(ApplyMaster);
        if (bgmSlider)    bgmSlider.onValueChanged.AddListener(ApplyBgm);
        if (sfxSlider)    sfxSlider.onValueChanged.AddListener(ApplySfx);
    }

    // === 열고 닫기 ===
    public void Open()
    {
        if (optionsRoot) optionsRoot.SetActive(true);
        // 인게임 일시정지 원하면: Time.timeScale = 0f;
    }

    public void Close()
    {
        SaveAll();
        if (optionsRoot) optionsRoot.SetActive(false);
        // Time.timeScale = 1f;
    }

    void Update()
    {
        if (optionsRoot && optionsRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    // === 볼륨 적용 ===
    public void ApplyMaster(float v)
    {
        SetMixerLinear(masterParam, v);
        // Mixer 전역이 없으면 AudioListener로 대체
        if (!audioMixer) AudioListener.volume = Mathf.Clamp01(v);
    }

    public void ApplyBgm(float v)
    {
        SetMixerLinear(bgmParam, v);
        // Mixer 미연동 보조
        if (bgmSource) bgmSource.volume = Mathf.Clamp01(v);
    }

    public void ApplySfx(float v)
    {
        SetMixerLinear(sfxParam, v);
        // 개별 SFX 매니저가 있다면 거기에 전달하도록 확장 가능
    }

    // 0~1 선형 → dB 변환 후 Mixer에 세팅
    void SetMixerLinear(string param, float linear01)
    {
        if (!audioMixer || string.IsNullOrEmpty(param)) return;

        float dB = (linear01 <= 0.0001f)
            ? -80f
            : Mathf.Log10(Mathf.Clamp01(linear01)) * 20f;

        // 파라미터 존재 여부 체크 겸 세팅
        bool ok = audioMixer.SetFloat(param, dB);
#if UNITY_EDITOR
        if (!ok)
            Debug.LogWarning($"[OptionsMenu] Mixer param not found: '{param}'");
#endif
    }

    void SaveAll()
    {
        if (masterSlider) PlayerPrefs.SetFloat(KEY_MASTER, masterSlider.value);
        if (bgmSlider)    PlayerPrefs.SetFloat(KEY_BGM,    bgmSlider.value);
        if (sfxSlider)    PlayerPrefs.SetFloat(KEY_SFX,    sfxSlider.value);
        PlayerPrefs.Save();
    }

    // === Quit ===
    public void QuitGame()
    {
        SaveAll();
        // Time.timeScale = 1f; // 일시정지 사용 시 복원

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  // 에디터에서는 플레이 중지
#elif UNITY_WEBGL
        Debug.Log("WebGL 빌드에선 Application.Quit()이 동작하지 않습니다.");
#else
        Application.Quit();                   // PC/Mobile 빌드에서 종료
#endif
    }

    // 타이틀로 복귀하고 싶을 때 쓸 수 있는 선택지
    public void QuitToTitle()
    {
        SaveAll();
        // Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }
}
