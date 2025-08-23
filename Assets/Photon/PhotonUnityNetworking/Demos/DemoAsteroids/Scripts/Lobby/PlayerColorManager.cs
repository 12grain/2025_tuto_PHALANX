using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;


namespace Photon.Pun.Demo.Asteroids
{
    public class PlayerColorManager : MonoBehaviour
    {

        public static PlayerColorManager instance;

        public string PlayerColor { get; private set; } // 현재 내 토글 상태 저장
        private Toggle myToggle;
        [SerializeField]
        private bool myToggleChecked;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (myToggle != null)
            {
                myToggleChecked = myToggle.isOn; 
            }
        }
        // PlayerListEntry에서 내 토글 생성 시 호출
        public void RegisterMyToggle(Toggle toggle)
        {
            myToggle = toggle;

            // 현재 토글 상태 기록
            PlayerColor = myToggle.isOn ? "white" : "black";
            myToggleChecked = myToggle.isOn;
            // 값 변경될 때마다 PlayerColor만 갱신
            myToggle.onValueChanged.AddListener((isOn) =>
            {
                PlayerColor = isOn ? "white" : "black";
                myToggleChecked = myToggle.isOn;
                Debug.Log($"[PlayerColorManager] 토글 변경 감지 → PlayerColor = {PlayerColor}");
            });

            Debug.Log($"[PlayerColorManager] 내 토글 등록 완료. 초기값: {PlayerColor}");
        }


        public bool GetMyToggle()
        {
            return myToggleChecked;
        }
    }
}