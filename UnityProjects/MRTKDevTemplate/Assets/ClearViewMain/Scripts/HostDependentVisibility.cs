using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;


namespace ClearView.Network
{

    public class HostDependentVisibility : MonoBehaviourPunCallbacks
    {
        [Header("Objects Controlled by Master Client")]
        public List<GameObject> masterClientOnlyObjects = new List<GameObject>();

        private void Start()
        {
            UpdateObjectVisibility();
        }

        public override void OnJoinedRoom()
        {
            UpdateObjectVisibility(); // Update visibility when the user joins a room
        }

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            UpdateObjectVisibility(); // Update visibility if the master client changes
        }

        public override void OnLeftRoom()
        {
            UpdateObjectVisibility(); // Update visibility when the user leaves a room
        }

        private void UpdateObjectVisibility()
        {
            if (PhotonNetwork.InRoom)
            {
                // Check if the current client is the master client
                bool isMasterClient = PhotonNetwork.IsMasterClient;

                // Loop through the list of objects and set their active state
                foreach (GameObject obj in masterClientOnlyObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(isMasterClient);
                    }
                }
            }
            else
            {
                // If the user is not in a room, set all objects to active
                foreach (GameObject obj in masterClientOnlyObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }
            }
        }
    }
}

