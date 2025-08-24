using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 이 스크립트를 어디서든 쉽게 접근할 수 있도록 하는 '싱글턴' 패턴
    public static SoundManager Instance;

    // Inspector 창에서 할당할 오디오 클립들
    public AudioClip moveSound;
    public AudioClip killSound;

    // 실제 소리를 재생할 컴포넌트
    private AudioSource audioSource;

    void Awake()
    {
        // 싱글턴 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 이 오브젝트에 붙어있는 AudioSource 컴포넌트를 가져옴
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMoveSound()
    {
        if (moveSound != null)
        {
            // PlayOneShot은 여러 사운드가 겹쳐서 재생될 수 있게 해줌
            audioSource.PlayOneShot(moveSound);
        }
    }

    public void PlayKillSound()
    {
        if (killSound != null)
        {
            audioSource.PlayOneShot(killSound);
        }
    }
}