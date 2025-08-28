using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    // 효과음(SFX) 클립들
    public AudioClip moveSound;
    public AudioClip killSound;
    public AudioClip fusionSound;

    // 게임 씬 BGM 클립
    public AudioClip gameBGM;

    private AudioSource sfxSource;
    private AudioSource bgmSource; // 게임 씬 BGM을 위한 스피커

    void Awake()
    {
        Instance = this;
        // SoundManager 오브젝트에 있는 모든 AudioSource를 가져옴
        AudioSource[] sources = GetComponents<AudioSource>();
        sfxSource = sources[0];
        // 만약 게임 씬 전용 BGM도 있다면, 두 번째 AudioSource를 사용
        if (sources.Length > 1)
        {
            bgmSource = sources[1];
        }
    }

    void Start()
    {
        // 로비 BGM은 멈추고, 게임 BGM을 시작
        if (BGMManager.instance != null)
        {
            BGMManager.instance.gameObject.SetActive(false); // 로비 BGM 매니저를 비활성화
        }
        if (bgmSource != null && gameBGM != null)
        {
            bgmSource.clip = gameBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // 아래 함수들은 이제 sfxSource를 사용하도록 변경
    public void PlayMoveSound()
    {
        if (moveSound != null)
        {
            sfxSource.PlayOneShot(moveSound);
        }
    }

    public void PlayKillSound()
    {
        if (killSound != null)
        {
            sfxSource.PlayOneShot(killSound);
        }
    }

    public void PlayFusionSound()
    {
        if (fusionSound != null)
        {
            sfxSource.PlayOneShot(fusionSound);
        }
    }
}