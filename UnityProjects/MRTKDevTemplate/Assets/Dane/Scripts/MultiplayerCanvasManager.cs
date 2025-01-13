using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using ClearView.Network;
using MixedReality.Toolkit.UX;


namespace ClearView.Network
{
    // Uses the PhotonNetwork API to manage the state of the multiplayer canvas
    // Tries to simplify the interface to make sure that users can only do the right things at any given time
    // The actual actions are handled by the NetworkManager
    public class MultiplayerCanvasManager : MonoBehaviourPunCallbacks
    {
        public bool isConnectedToMaster => PhotonNetwork.IsConnectedAndReady;
        public bool isConnectedToRoom => PhotonNetwork.InRoom;
        public bool isConnectedToLobby => PhotonNetwork.InLobby;


        public GameObject playerListPanel; // conditionally enable/disable this panel based on in-room status
        public PressableButton joinRoomButton; // conditionally enable/disable this button based on in-room status

        public TMP_Text joinRoomButtonText; // conditionally change this text based on master client status
        public string connectToMasterText = "Connect as the Master Client";
        public string connectingTest = "Connecting...";
        public string connectToRoomText = "Connect to Default Room";

        private bool isConnecting => NetworkManager.Instance.isBusy;

        public void LateUpdate()
        {
            if (joinRoomButtonText)
            {
                if (isConnectedToMaster && joinRoomButtonText.text != connectToRoomText)
                {
                    joinRoomButtonText.text = connectToRoomText;
                }

                if (!isConnectedToMaster && joinRoomButtonText.text != connectToMasterText)
                {
                    joinRoomButtonText.text = connectToMasterText;
                }

                if (isConnecting)
                {
                    joinRoomButtonText.text = PhotonNetwork.NetworkClientState.ToString();
                    if (joinRoomButton && joinRoomButton.enabled) joinRoomButton.enabled = false;
                }

                if (joinRoomButton && !joinRoomButton.enabled) joinRoomButton.enabled = true;
            }


            if (isConnectedToRoom)
            {
                if (playerListPanel && !playerListPanel.activeSelf) playerListPanel.SetActive(true);
                if (joinRoomButton && joinRoomButton.gameObject.activeSelf) joinRoomButton.gameObject.SetActive(false);
            }
            else
            {
                if (playerListPanel && playerListPanel.activeSelf) playerListPanel.SetActive(false);
                if (joinRoomButton && !joinRoomButton.gameObject.activeSelf) joinRoomButton.gameObject.SetActive(true);       
            }
        }
    }
}
