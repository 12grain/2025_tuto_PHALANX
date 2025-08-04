// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerListEntry.cs" company="Exit Games GmbH">
//   Part of: Asteroid Demo,
// </copyright>
// <summary>
//  Player List Entry
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

namespace Photon.Pun.Demo.Asteroids
{
    public class PlayerListEntry : MonoBehaviour
    {
        [Header("UI References")]
        public Text PlayerNameText;

        public Image PlayerColorImage; 
        public Toggle PlayerBlackWhiteToggle;
        public Button PlayerReadyButton;
        public Image PlayerReadyImage;
       

        private int ownerId;
        private bool isPlayerReady;

        #region UNITY

        public void OnEnable()
        {
            PlayerNumbering.OnPlayerNumberingChanged += OnPlayerNumberingChanged;
        }

        public void Start()
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
            {
                PlayerReadyButton.gameObject.SetActive(false);
                PlayerBlackWhiteToggle.gameObject.GetComponent<Toggle>().interactable = false; //내 토글이 아니면 못 만지게 
            }
            else
            {
                string initialColor = PlayerBlackWhiteToggle.isOn ? "white" : "black";
                SetPlayerColor(initialColor);

                /* Hashtable initialProps = new Hashtable() {{AsteroidsGame.PLAYER_READY, isPlayerReady}, {AsteroidsGame.PLAYER_LIVES, AsteroidsGame.PLAYER_MAX_LIVES}};
                 PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
                 PhotonNetwork.LocalPlayer.SetScore(0);

                 PlayerReadyButton.onClick.AddListener(() =>
                 {
                     isPlayerReady = !isPlayerReady;
                     SetPlayerReady(isPlayerReady);

                     Hashtable props = new Hashtable() {{AsteroidsGame.PLAYER_READY, isPlayerReady}};
                     PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                     if (PhotonNetwork.IsMasterClient)
                     {
                         #if UNITY_6000_0_OR_NEWER
                         FindFirstObjectByType<LobbyMainPanel>().LocalPlayerPropertiesUpdated();
                         #else
                         FindObjectOfType<LobbyMainPanel>().LocalPlayerPropertiesUpdated();
                         #endif
                     }
                 });*/

                PlayerReadyButton.onClick.AddListener(() =>
                {
                    isPlayerReady = !isPlayerReady;
                    SetPlayerReady(isPlayerReady);

                    Hashtable props = new Hashtable { { AsteroidsGame.PLAYER_READY, isPlayerReady } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                    if (PhotonNetwork.IsMasterClient)
                    {
#if UNITY_6000_0_OR_NEWER
                        FindFirstObjectByType<LobbyMainPanel>().LocalPlayerPropertiesUpdated();
#else
                        FindObjectOfType<LobbyMainPanel>().LocalPlayerPropertiesUpdated();
#endif
                    }
                });

                // Toggle 리스너 추가
                PlayerBlackWhiteToggle.onValueChanged.AddListener(OnColorToggleChanged);
            

            // 초기 상태 동기화
            Hashtable initialProps = new Hashtable
            {
                { AsteroidsGame.PLAYER_READY, isPlayerReady },
                { AsteroidsGame.PLAYER_LIVES, AsteroidsGame.PLAYER_MAX_LIVES }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
            PhotonNetwork.LocalPlayer.SetScore(0);
            }

        }
        

        public void OnDisable()
        {
            PlayerNumbering.OnPlayerNumberingChanged -= OnPlayerNumberingChanged;
        }

        #endregion

        public void Initialize(int playerId, string playerName)
        {
            ownerId = playerId;
            PlayerNameText.text = playerName;
        }

        private void OnPlayerNumberingChanged()
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.ActorNumber == ownerId)
                {
                    PlayerColorImage.color = AsteroidsGame.GetColor(p.GetPlayerNumber());
                }
            }
        }
        public void SetPlayerReady(bool playerReady)
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = playerReady ? "Ready!" : "Ready?";
            PlayerReadyImage.enabled = playerReady;
        }

        public void OnColorToggleChanged(bool isOn)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId) return;

            string color = isOn ? "white" : "black";
            SetPlayerColor(color);
            UpdateOtherToggleUIVisual(color);
        }

        public void SetPlayerColor(string color)
        {
            PlayerPrefs.SetString("MyColor", color);

            Hashtable props = new Hashtable
    {
        { AsteroidsGame.PLAYER_COLOR, color }
    };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public void UpdateColorFromProperties(Player player)
        {
            object colorValue;
            if (player.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_COLOR, out colorValue))
            {
                string color = (string)colorValue;

                // 내가 아닌 다른 플레이어의 색이면 반대 색을 선택할 수 없게 처리
                if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    string myColor = color == "white" ? "black" : "white";

                    // 상대가 흰색이면, 나는 흑색 토글로 강제
                    bool isOn = myColor == "white";
                    PlayerBlackWhiteToggle.isOn = isOn;

                    // 동기화를 위해 내 상태도 갱신 (이미 선택 안한 경우만)
                    if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(AsteroidsGame.PLAYER_COLOR))
                    {
                        SetPlayerColor(myColor);
                    }
                }
            }
        }

        public void UpdateOtherToggleUIVisual(string myColor)
        {
            foreach (var kvp in FindObjectOfType<LobbyMainPanel>().playerListEntries)
            {
                if (kvp.Key != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    var otherEntry = kvp.Value.GetComponent<PlayerListEntry>();

                    // 리스너 잠시 제거 (루프 방지)
                    otherEntry.PlayerBlackWhiteToggle.onValueChanged.RemoveAllListeners();

                    // 내 선택과 반대 색으로 토글 UI만 반영
                    bool otherIsWhite = (myColor == "black");  // 내가 black이면 상대는 white
                    otherEntry.PlayerBlackWhiteToggle.isOn = otherIsWhite;

                    // 다시 리스너 복구 (상대방은 interactable=false라 생략 가능)
                    otherEntry.PlayerBlackWhiteToggle.onValueChanged.AddListener(otherEntry.OnColorToggleChanged);
                }
            }
        }
    }
}