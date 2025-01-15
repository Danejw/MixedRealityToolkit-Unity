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

        // UI
        public ModelDetailsPanel modelDetailsPanel;
        public Transform roomCenter;

        // Clipping Tool
        public Transform toolSnap;
        public GameObject clippingToolPrefab;
        [SerializeField] private GameObject clippingToolInstance;
        public ClippingPrimitive clippingTool;

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
        }

        public override void OnEnable()
        {
            base.OnEnable();

            SetUpClippingTool();
        }

        // Network Events
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
        }



        // Check the available models if a model with said name exists, if it does, set it to active and set the current model to it
        public void SwitchTo(string name) // None, Brain, Heart, Aorta, etc.
        {
                if (name == "None")
                {
                    // Set every other model to false
                    foreach (var model in instantiatedModels)
                    {
                        model.SetActive(false);
                    }

                    modelDetailsPanel?.Close();
                    ToggleClippingTool(false);

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

            // Notify clients about the new model
            if (isOnline) photonView.RPC("SetCurrentModelRPC", RpcTarget.Others, modelPrefab.name);

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


            // Notify clients about the new model
            if (isOnline) photonView.RPC("SetCurrentModelRPC", RpcTarget.Others, modelPrefab.name + "(Clone)");

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

            // toggle the button list

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




        // RPC Recieves
        [PunRPC]
        private void SetCurrentModelRPC(string modelName)
        {
            // Find the model locally
            GameObject model = GameObject.Find(modelName);

            if (model != null)
            {
                model.transform.parent = transform; // Set the parent to keep the hierarchy organized
                instantiatedModels.Add(model);

                // Set it as the current model
                modelDetailsPanel?.SetModel(model);
                UpdateClippingToolRenderers(model);

                currentModel = model;

                Logger.Log(Logger.Category.Info, $"Current model set to '{modelName}'");
            }
            else
            {
                Logger.Log(Logger.Category.Error, $"Model with name '{modelName}' not found.");
            }
        }


        [PunRPC]
        private void ActivateClippingToolRPC(string name, bool isActive)
        {
           // find object by type
           var clip = FindObjectOfType<ClippingPrimitive>();

                if (clip != null)
                {
                    clippingToolInstance = clip.gameObject;

                    clippingToolInstance?.TryGetComponent<ClippingPrimitive>(out clippingTool);
                    clippingToolInstance.transform.parent = transform;


                    if (currentModel) UpdateClippingToolRenderers(currentModel);

                    clippingToolInstance.gameObject.SetActive(true);

                    Logger.Log(Logger.Category.Info, "Clipping tool activated.");
                }
                else
                {
                    Logger.Log(Logger.Category.Error, $"Gameobject {name} was not found.");
                }
            
        }




        // TODO: Clipping tool management should be moved to a separate class
        public void ToggleClippingTool(bool isActive)
        {
            if (isActive)
            {
                // Check if the tool snap transform is assigned
                if (toolSnap == null)
                {
                    Logger.Log(Logger.Category.Error, "Tool snap transform is missing.");
                    return;
                }

                SetUpClippingTool();

                // Set the position and rotation of the clipping tool to match the tool snap transform
                clippingTool.transform.position = toolSnap.position;
                clippingTool.transform.rotation = toolSnap.rotation;

                // Activate or deactivate the clipping tool based on the isActive parameter
                clippingTool?.gameObject.SetActive(true);
            }
            else
            {
                clippingTool?.gameObject.SetActive(false);
            }

            // Notify clients about the new model
            if (isOnline) photonView.RPC("ActivateClippingToolRPC", RpcTarget.Others, clippingToolPrefab.name + "(Clone)", isActive);
        }

        private void SetUpClippingTool()
        {
            // If the player is online and the clipping tool instance is not a photon instantiated then destroy the object and instantiate it using photon network
            // If the player is online and the clipping tool instance is a photon instantiated object then continue
            // If the player is offline and the clipping tool instance is a photon instantiated object then destroy the object and instantiate it locally
            // If the player is offline and the clipping tool instance is not a photon instantiated object then continue
            if (clippingToolInstance) Destroy(clippingToolInstance);

            clippingToolInstance = isOnline
                ? PhotonNetwork.IsMasterClient ? PhotonNetwork.Instantiate(clippingToolPrefab.name, toolSnap.transform.position, toolSnap.transform.rotation)
                : null
                : Instantiate(clippingToolPrefab, toolSnap.transform.position, toolSnap.transform.rotation, transform);

            if (clippingToolInstance != null)
            {
                // Assign the name consistently across all instances
                clippingToolInstance.name = clippingToolPrefab.name + "(Clone)";

                // Try to get and set up the ClippingPlane component
                clippingToolInstance.TryGetComponent(out clippingTool);
                clippingToolInstance.transform.parent = transform;

                if (currentModel) UpdateClippingToolRenderers(currentModel);

                // Ensure the tool is initially inactive
                clippingToolInstance.gameObject.SetActive(false);
            }
        }

        public void UpdateClippingToolRenderers(GameObject model)
        {
            // set child renderers into the clipping tool
            clippingTool?.ClearRenderers();

            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            {
                clippingTool?.AddRenderer(renderer);
            }
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

