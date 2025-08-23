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

        public string PlayerColor { get; private set; } // ���� �� ��� ���� ����
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
        // PlayerListEntry���� �� ��� ���� �� ȣ��
        public void RegisterMyToggle(Toggle toggle)
        {
            myToggle = toggle;

            // ���� ��� ���� ���
            PlayerColor = myToggle.isOn ? "white" : "black";
            myToggleChecked = myToggle.isOn;
            // �� ����� ������ PlayerColor�� ����
            myToggle.onValueChanged.AddListener((isOn) =>
            {
                PlayerColor = isOn ? "white" : "black";
                myToggleChecked = myToggle.isOn;
                Debug.Log($"[PlayerColorManager] ��� ���� ���� �� PlayerColor = {PlayerColor}");
            });

            Debug.Log($"[PlayerColorManager] �� ��� ��� �Ϸ�. �ʱⰪ: {PlayerColor}");
        }


        public bool GetMyToggle()
        {
            return myToggleChecked;
        }
    }
}