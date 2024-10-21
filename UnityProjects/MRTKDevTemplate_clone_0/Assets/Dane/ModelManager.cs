using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;


namespace ClearView
{
    public class ModelManager : MonoBehaviour
    {
        public ModelDetailsPanel modelDetailsPanel;
        public Transform roomCenter;
        public List<GameObject> availableModels;
        public List<GameObject> instantiatedModels;

        public void InstantiateModel(string name)
        {
            // Ensure that only the host can instantiate models
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("Only the host (MasterClient) can instantiate models.");
                return;
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
            GameObject instantiatedModel = PhotonNetwork.Instantiate(modelPrefab.name, roomCenter.position, Quaternion.identity);
            instantiatedModel.transform.parent = transform; // Set the parent to keep the hierarchy organized
            instantiatedModels.Add(instantiatedModel);

            // Update the UI panel with model details
            modelDetailsPanel?.SetModel(modelPrefab);
        }

        public void RemoveModels()
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

            instantiatedModels.Clear(); // Clear the list of instantiated models
        }

        public void InstantiateBrain()
        {
            InstantiateModel("Brain");
        }

        public void InstantiateHeart()
        {
            InstantiateModel("Heart");
        }
    }
}
