using GLTFast;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.GraphicsTools;
using ClearView.Network;

namespace ClearView
{
    // This is where we will manage all of the models available to the player at any given moment
    public class ModelManager : MonoBehaviourPunCallbacks
    {
        // State
        public bool isOnline => PhotonNetwork.InRoom;
        public bool instantiateMode = false;

        // UI
        public ModelDetailsPanel modelDetailsPanel;
        public Transform roomCenter;

        // Clipping Tool
        public Transform toolSnap;
        public GameObject clippingToolPrefab;
        private GameObject clippingToolInstanceOnline;
        private GameObject clippingToolInstanceOffline;
        public ClippingPlane clippingTool;

        // Models Storage
        public GameObject currentModel;
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

        // Helper for testing
        public string nameOfModelToInstantiate;

        private void Start()
        {
            if (App.Instance.OneDriveManager)
            {
                App.Instance.OneDriveManager.OnImportComplete += OnImportComplete;
                App.Instance.OneDriveManager.OnInitialize += GetOneDriveFiles;
            }

            SetUpClippingTool();
        }

        private void SetUpClippingTool()
        {
            // TODO: Clipping tool management should be moved to a separate class
            if (PhotonNetwork.IsConnected)
            {
                if (!clippingToolInstanceOnline) clippingToolInstanceOnline = PhotonNetwork.Instantiate(clippingToolPrefab.name, toolSnap.transform.position, toolSnap.transform.rotation);
                clippingToolInstanceOnline.TryGetComponent<ClippingPlane>(out clippingTool);
                clippingToolInstanceOnline.transform.parent = transform;
                clippingToolInstanceOnline.gameObject.SetActive(false);
            }
            else
            {
                if (!clippingToolInstanceOffline) clippingToolInstanceOffline = Instantiate(clippingToolPrefab, toolSnap.transform.position, toolSnap.transform.rotation, transform);
                clippingToolInstanceOffline.TryGetComponent<ClippingPlane>(out clippingTool);
                clippingToolInstanceOffline.transform.parent = transform;
                clippingToolInstanceOffline.gameObject.SetActive(false);
            }
        }

        // Network Events
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            SetUpClippingTool();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            SetUpClippingTool();
        }

        // Check the available models if a model with said name exists, if it does, set it to active and set the current model to it
        public void SwitchTo(string name) // None, Brain, Heart, Aorta, etc.
        {
            if (!instantiateMode)
            {
                if (name == "None")
                {
                    if (!instantiateMode)
                    {
                        // Set every other model to false
                        foreach (var model in availableModels)
                        {
                            model.SetActive(false);
                        }

                        modelDetailsPanel?.Close();
                    }
                    return;
                }


                if (isOnline)
                {
                    // Find the model with the specified name
                    GameObject modelPrefab = Resources.Load<GameObject>(name);

                    if (modelPrefab == null)
                    {
                        Logger.Log(Logger.Category.Error, $"Model with name '{name}' not found in our resources folder.");
                        return;
                    }

                    // If online, instantiate the model using Photon
                    InstantiateModelOnline(modelPrefab);
                }
                else
                {
                    // Find the model with the specified name
                    GameObject modelPrefab = availableModels.Find(m => m.name == name);

                    if (modelPrefab == null)
                    {
                        Logger.Log(Logger.Category.Error, $"Model with name '{name}' not found in our available models.");
                        return;
                    }


                    // If offline, switch to the model
                    SwitchToOfflineModel(modelPrefab);
                }
            }
        }

        // Functionality
        public void InstantiateModel(string name)
        {
            if (isOnline)
            {
                // Ensure that only the host can instantiate models
                if (!PhotonNetwork.IsMasterClient)
                {
                    Logger.Log(Logger.Category.Info, "Only the host (MasterClient) can instantiate models.");
                    return;
                }
            }

            // Find the model with the specified name
            GameObject modelPrefab = availableModels.Find(m => m.name == name);

            if (modelPrefab == null)
            {
                Logger.Log(Logger.Category.Error, $"Model with name '{name}' not found in available models.");
                return;
            }

            // Check if the model is already instantiated
            if (instantiatedModels.Contains(modelPrefab))
            {
                Logger.Log(Logger.Category.Warning, $"Model '{name}' is already instantiated.");
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
            modelDetailsPanel?.SetModel(instantiatedModel);
            UpdateClippingToolRenderers(instantiatedModel);

            currentModel = instantiatedModel;
        }

        private void InstantiateModelOnline(GameObject modelPrefab)
        {
            // Ensure that only the host can instantiate models
            if (!PhotonNetwork.IsMasterClient)
            {
                Logger.Log(Logger.Category.Info, "Only the host (MasterClient) can instantiate models.");
                return;
            }

            // Destroy other models before instantiating a new one
            RemoveModels();

            // Instantiate the model using Photon
            GameObject instantiatedModel = PhotonNetwork.Instantiate(modelPrefab.name, roomCenter.position, Quaternion.identity);
            instantiatedModel.transform.parent = transform; // Set the parent to keep the hierarchy organized
            instantiatedModels.Add(instantiatedModel);

            // Update the UI panel with model details
            modelDetailsPanel?.SetModel(instantiatedModel);
            UpdateClippingToolRenderers(instantiatedModel);

            currentModel = instantiatedModel;
        }

        private void SwitchToOfflineModel(GameObject modelPrefab)
        {
            currentModel = modelPrefab;
            if (!instantiatedModels.Contains(modelPrefab)) instantiatedModels.Add(modelPrefab);

            // Set every other model to false
            foreach (var model in availableModels)
            {
                if (model != modelPrefab)
                {
                    model.SetActive(false);
                }
                else
                {
                    model.SetActive(true);
                    // Update the UI panel with model details
                    modelDetailsPanel?.SetModel(model);

                    // set child renderers into the clipping tool
                    UpdateClippingToolRenderers(model);
                }
            }
        }


        public void UpdateClippingToolRenderers(GameObject model)
        {
            // set child renderers into the clipping tool
            clippingTool?.renderers.Clear();
            clippingTool?.renderers.RemoveAll(item => item == null); // Removes null references

            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            {
                clippingTool?.renderers.Add(renderer);
            }
        }
        

        public void RemoveModels()
        {
            
            if (isOnline)
            {
                // Ensure that only the host can remove models
                if (!PhotonNetwork.IsMasterClient)
                {
                    Logger.Log(Logger.Category.Warning, "Only the host (MasterClient) can remove models.");
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
                        Logger.Log(Logger.Category.Warning, $"Failed to destroy model '{model.name}'. Only the owner or MasterClient can destroy networked objects.");
                    }
                }
            }
            
            else
            {
                // Destroy all instantiated models
                foreach (var model in instantiatedModels)
                {
                    model.SetActive(false);
                    //Destroy(model);
                }
            }
            
            

            instantiatedModels.Clear(); // Clear the list of instantiated models

            currentModel = null;
        }

        public void AddModel(GameObject go)
        {
            availableModels.Add(go);
        }

        // Toggles the clipping tool's active state and positions it at the tool snap location.
        public void ToggleClippingTool(bool isActive)
        {
            // Check if the clipping tool is assigned
            if (clippingTool == null)
            {
                Logger.Log(Logger.Category.Error, "Clipping tool is missing.");
                return;
            }

            // Check if the tool snap transform is assigned
            if (toolSnap == null)
            {
                Logger.Log(Logger.Category.Error, "Tool snap transform is missing.");
                return;
            }

            // Set the position and rotation of the clipping tool to match the tool snap transform
            clippingTool.transform.position = toolSnap.position;
            clippingTool.transform.rotation = toolSnap.rotation;

            // Activate or deactivate the clipping tool based on the isActive parameter
            clippingTool?.gameObject.SetActive(isActive);
        }

        // Test
        public void InstantiateModel()
        {
            if (string.IsNullOrEmpty(nameOfModelToInstantiate))
            {
                Logger.Log(Logger.Category.Error, "Name of model to instantiate is missing.");
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
        public async void GetOneDriveFiles()
        {
            OnlineModels = await App.Instance.OneDriveManager.ListAllFilesInOneDrive();

            // foreach (var model in onlineModels) Debug.Log($"Model: {model.Key}, ID: {model.Value}");
        }

        public async void ImportModel(string filename)
        {
            if (!OnlineModels.ContainsKey(filename)) return;

            await App.Instance.OneDriveManager.DownloadFromOneDrive(filename, OnlineModels[filename]);

            Logger.Log(Logger.Category.Info, $"Importing {filename}");
        }

        private void OnImportComplete(GltfImport import, GameObject go)
        {
            RemoveModels(); // Destroy other models before instantiating a new one

            import.InstantiateMainScene(go.transform);

            go.transform.parent = transform; // Set the parent to keep the hierarchy organized
            Vector3.Lerp(transform.position, roomCenter.position, 1 * Time.deltaTime); // Set the position to the center of the room
            go.transform.position = transform.position; // Set the position to the center of the room
            go.transform.rotation = Quaternion.identity;

            // Check for network components or add
            if (isOnline)
            {
                // Setup Photon View
                go.TryGetComponent<PhotonView>(out var view);
                if (view == null) view = go.AddComponent<PhotonView>();

                // Setup Photon Transform View
                go.TryGetComponent<PhotonTransformView>(out var tview);
                if (tview == null)
                {
                    tview = go.AddComponent<PhotonTransformView>();
                    tview.m_SynchronizeScale = true;
                    tview.m_SynchronizePosition = true;
                    tview.m_SynchronizeRotation = true;
                }
                else
                {
                    tview.m_SynchronizeScale = true;
                    tview.m_SynchronizePosition = true;
                    tview.m_SynchronizeRotation = true;
                }
            }

            instantiatedModels.Add(go);

            // Update the UI panel with model details
            modelDetailsPanel?.SetModel(go);
            UpdateClippingToolRenderers(go);

            AddModel(go);

            Logger.Log(Logger.Category.Info, $"Imported {go.name}");
        }
    }
}
