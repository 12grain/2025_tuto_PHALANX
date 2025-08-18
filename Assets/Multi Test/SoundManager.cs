using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // �� ��ũ��Ʈ�� ��𼭵� ���� ������ �� �ֵ��� �ϴ� '�̱���' ����
    public static SoundManager Instance;

    // Inspector â���� �Ҵ��� ����� Ŭ����
    public AudioClip moveSound;
    public AudioClip killSound;

    // ���� �Ҹ��� ����� ������Ʈ
    private AudioSource audioSource;

    void Awake()
    {
        // �̱��� ����
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // �� ������Ʈ�� �پ��ִ� AudioSource ������Ʈ�� ������
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMoveSound()
    {
        if (moveSound != null)
        {
            // PlayOneShot�� ���� ���尡 ���ļ� ����� �� �ְ� ����
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