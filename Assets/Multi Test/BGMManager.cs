using UnityEngine;

public class BGMManager : MonoBehaviour
{
    // BGMManager를 게임 전체에서 단 하나만 존재하도록 만드는 '싱글턴' 패턴
    public static BGMManager instance;

    private AudioSource bgmSource;

    void Awake()
    {
        // 만약 instance가 아직 없다면, 자기 자신을 instance로 지정
        if (instance == null)
        {
            instance = this;
            // 씬이 바뀌어도 이 오브젝트는 파괴되지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }
        // 만약 instance가 이미 존재한다면 (다른 씬에서 넘어온 BGMManager가 있다면)
        else
        {
            // 새로 생긴 자기 자신을 파괴해서 중복을 막음
            Destroy(gameObject);
        }

        bgmSource = GetComponent<AudioSource>();
    }

    // 볼륨 조절이 필요할 경우를 대비한 함수 (선택 사항)
    public void SetVolume(float volume)
    {
        bgmSource.volume = volume;
    }
}