using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using dotenv.net;
using System;


namespace ClearView.Network
{
    // This is where we will handle all of the basic network logic
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public bool loadFromEnvironmentVariable = true;


        public GameObject playerPrefab;

        [Space]
        public List<Transform> spawnPoints;


        public string roomName;


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


            // Check if the PUN APP ID is in the env folder
            if (loadFromEnvironmentVariable) LoadEnvVariable();
        }

        public void LoadEnvVariable()
        {
            ServerSettings settings = null;
            // Get the photon server settings
            try
            {
                settings = Resources.Load<ServerSettings>("PhotonServerSettings");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            // Load env file
            try
            {
                // Load .env file in the Unity project directory
                DotEnv.Load();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            // Set up PUN APP ID from env variable
            try
            {
                var id = Environment.GetEnvironmentVariable("PHOTON_PUN_APP_ID");

                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogError("PUN ID is missing. Please set PHOTON_PUN_APP_ID in your .env file.");
                }
                else
                {
                    if (settings) settings.AppSettings.AppIdRealtime = id;
                    Debug.Log($"PUN ID loaded");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load .env file: {ex.Message}");
            }

            // Set up VOICE APP ID from env variable
            try
            {
                var id = Environment.GetEnvironmentVariable("PHOTON_VOICE_APP_ID");

                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogError("VOICE ID is missing. Please set PHOTON_VOICE_APP_ID in your .env file.");
                }
                else
                {
                    if (settings) settings.AppSettings.AppIdVoice = id;
                    Debug.Log($"VOICE ID loaded");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load .env file: {ex.Message}");
            }

            // Set up FUSION APP ID from env variable
            try
            {
                var id = Environment.GetEnvironmentVariable("PHOTON_FUSION_APP_ID");

                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogError("FUSION ID is missing. Please set PHOTON_FUSION_APP_ID in your .env file.");
                }
                else
                {
                    if (settings) settings.AppSettings.AppIdFusion = id;
                    Debug.Log($"FUSION ID loaded");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load .env file: {ex.Message}");
            }


            // TODO: Set up CHAT from an env variable
        }


        private void Start()
        {
            //ConnectToMaster();
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

            GameObject _player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].position, Quaternion.identity);
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
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                // If not connected, connect to Photon
                ConnectToMaster();
            }

            PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
        }

        public void JoinOrCreateRoom()
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                // If not connected, connect to Photon
                ConnectToMaster();
            }

            // If connected, join or create the room
            PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
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
