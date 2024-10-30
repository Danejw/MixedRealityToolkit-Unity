using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


namespace ClearView.Network
{
    // THis is where we will handle all of the basic network logic
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public GameObject playerPrefab;
        public GameObject hostUIPrefab;
        public GameObject guestUIPrefab;

        // Make this a singleton
        public static NetworkManager Instance;

        public Player player;
        public static bool isHost;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        [Space]
        public List<Transform> spawnPoints;


        public string roomName;


        private void Start()
        {
            ConnectToMaster();
        }

        private void ConnectToMaster()
        {
            Debug.Log("Connecting...");
            PhotonNetwork.ConnectUsingSettings();
        }

        // Callbacks
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            Debug.Log("Connected to Master");

            player = PhotonNetwork.LocalPlayer;
            player.NickName = App.Instance.MicrosoftAuth.GetUsername();
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();

            Debug.Log("Joined Lobby");
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            isHost = PhotonNetwork.IsMasterClient;

            if (isHost)
            {
                // Spawn the UI for the host
                //Instantiate(hostUIPrefab);
            }
            else
            {
                // Spawn the UI for the guest
                //Instantiate(guestUIPrefab);
            }

            Debug.Log("Joined Room");

            GameObject _player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[Random.Range(0, spawnPoints.Count)].position, Quaternion.identity);
            //_player.GetComponent<PlayerSetup>().IsLocalPlayer();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Debug.Log("Left Room");
        }

        public override void OnLeftLobby()
        {
            base.OnLeftLobby();
            Debug.Log("Left Lobby");
        }


        // Actions
        public void JoinOrCreateRoom(string roomName)
        {
            PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
        }

        public void JoinOrCreateRoom()
        {
            PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
        }

        public void JoinLobby()
        {
            PhotonNetwork.JoinLobby();
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}
