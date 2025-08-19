using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Photon.Pun.Demo.Asteroids
{
    public class LobbyTopPanel : MonoBehaviour
    {
        private readonly string connectionStatusMessage = "    Connection Status: ";
        

        [Header("UI References")]
        public Text ConnectionStatusText;
        public TextMeshProUGUI ConnectionStatusTMP;


        #region UNITY

        public void Update()
        {
            ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
            ConnectionStatusTMP.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
           
        }

        #endregion
    }
}