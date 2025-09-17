using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;


namespace ClearView
{
    public class NetworkCondition : MonoBehaviour
    {
        public bool isConnectedToMaster => PhotonNetwork.IsConnectedAndReady;
        public bool isConnectedToRoom => PhotonNetwork.InRoom;
        public bool isConnectedToLobby => PhotonNetwork.InLobby;


        public TMP_Text connectToRoomButtonText;
        public TMP_Text connectToRoomHandText;


        public void LateUpdate()
        {
            if (connectToRoomButtonText)
            {
                if (isConnectedToMaster && connectToRoomButtonText.text != "Connect to Default Room")
                {
                    connectToRoomButtonText.text = "Connect to Default Room";
                }
                else if (!isConnectedToMaster && connectToRoomButtonText.text != "Connecting as the Master Client")
                {
                    connectToRoomButtonText.text = "Connecting as the Master Client";
                }
            }

            if (connectToRoomHandText)
            {
                if (isConnectedToRoom && connectToRoomHandText.text != "Connected")
                {
                    connectToRoomHandText.text = "Connected";
                }
                else if (!isConnectedToRoom && connectToRoomHandText.text != "")
                {
                    connectToRoomHandText.text = "";
                }
            }
        }
    }
}
