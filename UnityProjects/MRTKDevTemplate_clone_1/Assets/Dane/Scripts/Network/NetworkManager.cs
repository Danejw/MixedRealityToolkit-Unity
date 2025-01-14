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


        public GameObject networkPrefab;

        public GameObject realPlayer;
        private GameObject networkedPlayer;



        public List<GameObject> networkPrefabs;

        [Space]
        public List<Transform> spawnPoints;


        public string roomName;

        public bool isBusy = false;


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
                Logger.Log(Logger.Category.Error, e.ToString());
            }

            // Set up PUN APP ID from env variable
            try
            {
                var id = Environment.GetEnvironmentVariable("PHOTON_PUN_APP_ID");

                if (string.IsNullOrEmpty(id))
                {
                    Logger.Log(Logger.Category.Error, "PUN ID is missing. Please set PHOTON_PUN_APP_ID in your .env file.");
                }
                else
                {
                    if (settings) settings.AppSettings.AppIdRealtime = id;
                    Logger.Log(Logger.Category.Info, "PUN ID loaded");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Category.Error, $"Failed to load .env file: {ex.Message}");
            }

            // Set up VOICE APP ID from env variable
            try
            {
                var id = Environment.GetEnvironmentVariable("PHOTON_VOICE_APP_ID");

                if (string.IsNullOrEmpty(id))
                {
                    Logger.Log(Logger.Category.Error, "VOICE ID is missing. Please set PHOTON_VOICE_APP_ID in your .env file.");
                }
                else
                {
                    if (settings) settings.AppSettings.AppIdVoice = id;
                    Logger.Log(Logger.Category.Info, "VOICE ID loaded");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Category.Error, $"Failed to load .env file: {ex.Message}");
            }

            // Set up FUSION APP ID from env variable
            try
            {
                var id = Environment.GetEnvironmentVariable("PHOTON_FUSION_APP_ID");

                if (string.IsNullOrEmpty(id))
                {
                    Logger.Log(Logger.Category.Error, "FUSION ID is missing. Please set PHOTON_FUSION_APP_ID in your .env file.");
                }
                else
                {
                    if (settings) settings.AppSettings.AppIdFusion = id;
                    Logger.Log(Logger.Category.Info, "FUSION ID loaded");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Category.Error, $"Failed to load .env file: {ex.Message}");
            }


            // TODO: Set up CHAT from an env variable
        }


        private void Start()
        {
            //ConnectToMaster();
        }

        private void ConnectToMaster()
        {
            Logger.Log(Logger.Category.Info, "Connecting...");
            PhotonNetwork.ConnectUsingSettings();

            isBusy = true;
        }


        // Callbacks
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            Logger.Log(Logger.Category.Info, "Connected to Master");

            player = PhotonNetwork.LocalPlayer;
            player.NickName = App.Instance.MicrosoftAuth?.GetUsername();

            isBusy = false;
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Logger.Log(Logger.Category.Info, $"New Master Client: {newMasterClient.NickName}");
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();

            Logger.Log(Logger.Category.Info, "Joined Lobby");

            isBusy = false;
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            var masterClient = PhotonNetwork.MasterClient;

            if (player.ActorNumber == masterClient.ActorNumber)
            {
                // Spawn the UI for the host
                //Instantiate(hostUIPrefab);
                isHost = true;
            }
            else
            {
                isHost = false;
                // Spawn the UI for the guest
                //Instantiate(guestUIPrefab);
            }

            // create and set the networked player to the real player
            networkedPlayer = PhotonNetwork.Instantiate(networkPrefab.name, realPlayer.transform.position, realPlayer.transform.rotation);

            // move the player to a random spawn point
            realPlayer.transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].position;

            // make the player face the center of the map only on the x and z axis
            var here = new Vector3(0, realPlayer.transform.position.y, 0);
            realPlayer.transform.LookAt(here);

            Logger.Log(Logger.Category.Info, "Joined Room");

            networkPrefabs.Add(networkedPlayer);

            isBusy = false;
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            if (player != null)
            {
                PhotonNetwork.Destroy(networkedPlayer);
                networkPrefabs.Remove(networkedPlayer);
            }

            Logger.Log(Logger.Category.Info, "Left Room");

            isBusy = false;
        }

        public override void OnLeftLobby()
        {
            base.OnLeftLobby();
            Logger.Log(Logger.Category.Info, "Left Lobby");

            isBusy = false;
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

            Logger.Log(Logger.Category.Info, $"Connecting to {roomName}...");

            isBusy = true;
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

            Logger.Log(Logger.Category.Info, $"Connecting to {roomName}...");

            isBusy = true;
        }

        public void JoinLobby()
        {
            PhotonNetwork.JoinLobby();

            Logger.Log(Logger.Category.Info, $"Connecting to lobby...");

            isBusy = true;
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();

            Logger.Log(Logger.Category.Info, $"Leaving room...");

            isBusy = true;
        }
    }
}
