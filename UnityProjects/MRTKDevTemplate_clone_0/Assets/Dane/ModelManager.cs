using GLTFast;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace ClearView
{
    // This is where we will manage the all of the mdodels available to the player at any given moment
    public class ModelManager : MonoBehaviourPunCallbacks
    {
        // State
        public bool isOnline = false;


        // UI
        public ModelDetailsPanel modelDetailsPanel;
        public Transform roomCenter;

        // Models Storage
        [SerializeField] private List<GameObject> availableModels = new List<GameObject>();
        [SerializeField] private List<GameObject> instantiatedModels = new List<GameObject>();


        public event Action<Dictionary<string, string>> OnlineModelsUpdated; // Tells UI to update
        private Dictionary<string, string> onlineModels = new Dictionary<string, string>(); // Filename, File id
        public Dictionary<string, string> OnlineModels
        {
            get { return onlineModels; }
            private set
            {
                onlineModels = value;
                OnlineModelsUpdated?.Invoke(onlineModels);
            }
        }   

        // Helper
        public string nameOfModelToInstantiate;

        // Import from OneDrive and managed by the ModelManager
        private App app;

        private void Start()
        {
            app = App.Instance;
            app.OneDriveManager.OnImportComplete += OnImportComplete;
            app.OneDriveManager.OnInitialize += InitOneDrive;
        }


        // Network Events
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            isOnline = true;
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            isOnline = false;
        }


        // Functionality
        public void InstantiateModel(string name)
        {
            if (isOnline)
            {
                // Ensure that only the host can instantiate models
                if (!PhotonNetwork.IsMasterClient)
                {
                    Debug.LogWarning("Only the host (MasterClient) can instantiate models.");
                    return;
                }
            }    

            // Find the model with the specified name
            GameObject modelPrefab = availableModels.Find(m => m.name == name);

            if (modelPrefab == null)
            {
                Debug.LogError($"Model with name '{name}' not found in available models.");
                return;
            }

            // Check if the model is already instantiated
            if (instantiatedModels.Contains(modelPrefab))
            {
                Debug.LogWarning($"Model '{name}' is already instantiated.");
                return;
            }

            // Destroy other models before instantiating a new one
            RemoveModels();

            // Instantiate the model
            GameObject instantiatedModel;
            if (isOnline)
            {
                instantiatedModel = PhotonNetwork.Instantiate(modelPrefab.name, roomCenter.position, Quaternion.identity);
            }
            else
            {
                instantiatedModel = Instantiate(modelPrefab, roomCenter.position, Quaternion.identity);
            }

            instantiatedModel.transform.parent = transform; // Set the parent to keep the hierarchy organized
            instantiatedModels.Add(instantiatedModel);

            // Update the UI panel with model details
            modelDetailsPanel?.SetModel(modelPrefab);
        }

        public void RemoveModels()
        {
            if (isOnline)
            {
                // Ensure that only the host can remove models
                if (!PhotonNetwork.IsMasterClient)
                {
                    Debug.LogWarning("Only the host (MasterClient) can remove models.");
                    return;
                }

                // Destroy all instantiated models
                foreach (var model in instantiatedModels)
                {
                    // Get the PhotonView component to verify ownership
                    PhotonView photonView = model.GetComponent<PhotonView>();

                    // Only the owner or MasterClient can destroy the model
                    if (photonView != null && (photonView.IsMine || PhotonNetwork.IsMasterClient))
                    {
                        PhotonNetwork.Destroy(model);
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to destroy model '{model.name}'. Only the owner or MasterClient can destroy networked objects.");
                    }
                }
            }
            else
            {
                // Destroy all instantiated models
                foreach (var model in instantiatedModels)
                {
                    Destroy(model);
                }
            }

            instantiatedModels.Clear(); // Clear the list of instantiated models
        }

        public void AddModel(GameObject go)
        {
            availableModels.Add(go);
        }


        // Test
        public void InstantiateModel()
        {
            if (string.IsNullOrEmpty(nameOfModelToInstantiate))
            {
                Debug.LogError("Name of model to instantiate is missing.");
                return;
            }

            InstantiateModel(nameOfModelToInstantiate);
        }

        // Test
        public void InstantiateBrain()
        {
            InstantiateModel("Brain");
        }

        // Test
        public void InstantiateHeart()
        {
            InstantiateModel("Heart");
        }



        // Load all model info from OneDrive
        public async void InitOneDrive()
        {
            OnlineModels = await app.OneDriveManager.ListAllFilesInOneDrive();

            // foreach (var model in onlineModels) Debug.Log($"Model: {model.Key}, ID: {model.Value}");
        }

        public async void ImportModel(string filename)
        {
           if (!OnlineModels.ContainsKey(filename)) return;

            await app.OneDriveManager.DownloadAndLoadGLTF(filename, OnlineModels[filename]);

            Debug.Log($"Importing {filename}");
        }

        private void OnImportComplete(GltfImport import, GameObject go)
        {
            RemoveModels(); // Destroy other models before instantiating a new one

            import.InstantiateMainScene(go.transform);

            go.transform.parent = transform; // Set the parent to keep the hierarchy organized
            Vector3.Lerp(transform.position, roomCenter.position, 1 * Time.deltaTime); // Set the position to the center of the room
            go.transform.position = transform.position; // Set the position to the center of the room
            go.transform.rotation = Quaternion.identity;
            instantiatedModels.Add(go);

            // Update the UI panel with model details
            modelDetailsPanel?.SetModel(go);

            AddModel(go);

            Debug.Log($"Imported {go.name}");
        }
    }
}
