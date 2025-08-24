using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Photon.Pun.Demo.Asteroids
{
    public class RoomListEntry : MonoBehaviour
    {
       
        public Text RoomNameText; 
        public TextMeshProUGUI RoomNameTextPro;
        public Text RoomPlayersText;
        public TextMeshProUGUI RoomPlayersTextPro;
        public Button JoinRoomButton;

        private string roomName;

        public void Start()
        {
            JoinRoomButton.onClick.AddListener(() =>
            {
                if (PhotonNetwork.InLobby)
                {
                    PhotonNetwork.LeaveLobby();
                }

                PhotonNetwork.JoinRoom(roomName);
            });
        }

        public void Initialize(string name, byte currentPlayers, byte maxPlayers)
        {
            roomName = name;

    
            RoomNameTextPro.text = roomName;    
         
            RoomPlayersTextPro.text = currentPlayers + " / " + maxPlayers;
        }
    }
}