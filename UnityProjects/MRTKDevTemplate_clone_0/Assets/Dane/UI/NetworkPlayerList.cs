using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace ClearView.UI
{
    // This is where we will manage the UI for the list of players a the room
    public class NetworkPlayerList : MonoBehaviourPunCallbacks
    {
        public Transform parent;
        public GameObject networkItemPrefab;

        // Dictionary that stores player ActorNumber as keys and their UI elements as values
        private Dictionary<int, GameObject> playerListEntries = new Dictionary<int, GameObject>();

        private void Start()
        {
            if (parent == null) parent = transform;

            // Initialize the player list when the local player joins the room
            if (PhotonNetwork.InRoom) UpdatePlayerList();
        }

        // Update the player list when a player joins
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"{newPlayer.NickName} has joined the room.");
            UpdatePlayerList();
        }

        // Update the player list when a player leaves
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"{otherPlayer.NickName} has left the room.");
            UpdatePlayerList();
        }

        // Update the player list when the local player joins a room
        public override void OnJoinedRoom()
        {
            Debug.Log("Joined room. Updating player list.");
            UpdatePlayerList();
        }

        // Update the player list when the local player leaves a room
        public override void OnLeftRoom()
        {
            Debug.Log("Left room. Clearing player list.");
            ClearPlayerList();
        }

        // Updates the player list UI
        private void UpdatePlayerList()
        {
            // Clear the current UI elements without removing the player data
            ClearPlayerListUI();

            // Get the list of players in the room
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                AddPlayerToList(player);
            }
        }

        // Adds a player to the list UI
        private void AddPlayerToList(Player player)
        {
            if (parent == null || networkItemPrefab == null)
            {
                Debug.LogError("Player list container or player name prefab is not set.");
                return;
            }

            // Check if the player already has an entry in the dictionary
            if (!playerListEntries.ContainsKey(player.ActorNumber))
            {
                // If the player doesn't have an entry, create a new one
                string playerName = string.IsNullOrEmpty(player.NickName) ? GenerateRandomName() : player.NickName;

                // Instantiate the UI element for the player
                GameObject playerEntry = Instantiate(networkItemPrefab, parent.transform);
                playerEntry.GetComponent<NetworkPlayerListItem>().SetPlayerInfo(playerName, player.IsMasterClient, player.IsLocal);

                // Store the player info in the dictionary
                playerListEntries[player.ActorNumber] = playerEntry;
            }
            else
            {
                // If the player already has an entry, update the UI element only
                var playerInfo = playerListEntries[player.ActorNumber];
                playerInfo = Instantiate(networkItemPrefab, parent.transform);
                playerInfo.GetComponent<NetworkPlayerListItem>().SetPlayerInfo(player.NickName, player.IsMasterClient, player.IsLocal);
                playerListEntries[player.ActorNumber] = playerInfo;
            }
        }

        // Clears only the UI elements of the player list
        private void ClearPlayerListUI()
        {
            foreach (var entry in playerListEntries)
            {
                Destroy(entry.Value);
            }
        }

        // Clears the player list entirely (called when leaving the room)
        private void ClearPlayerList()
        {
            foreach (var entry in playerListEntries)
            {
                Destroy(entry.Value);
            }
            playerListEntries.Clear();
        }


        // Generate a random player name
        private string GenerateRandomName()
        {
            string[] adjectives = { "Brave", "Swift", "Clever", "Mighty", "Sneaky", "Loud", "Calm" };
            string[] nouns = { "Bear", "Fox", "Eagle", "Shark", "Lion", "Hawk", "Wolf" };

            string randomAdjective = adjectives[Random.Range(0, adjectives.Length)];
            string randomNoun = nouns[Random.Range(0, nouns.Length)];

            return $"{randomAdjective}{randomNoun}{Random.Range(100, 999)}";
        }
    }
}
