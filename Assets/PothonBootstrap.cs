using Photon.Pun;
using UnityEngine;

public class PhotonBootstrap : MonoBehaviour
{
    void Awake()
    {
        // 마스터가 PhotonNetwork.LoadLevel() 호출하면 모든 클라가 같은 씬으로 이동
        PhotonNetwork.AutomaticallySyncScene = true;

        // (선택) 이 오브젝트를 씬 전환해도 유지하고 싶으면
        // DontDestroyOnLoad(this.gameObject);
    }
}