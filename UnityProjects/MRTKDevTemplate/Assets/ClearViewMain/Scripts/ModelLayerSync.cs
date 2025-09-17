using Photon.Pun;
using UnityEngine;

namespace ClearView.Network
{
    public class ModelLayerSync : MonoBehaviourPun, IPunObservable
    {
        [Header("Assign your child layers here")]
        public GameObject[] layers; // Drag and drop your child layers in the Inspector

        private bool[] layerStates; // Array to store the visibility states of the layers

        private void Start()
        {
            // Initialize the layer states
            layerStates = new bool[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                layerStates[i] = layers[i].activeSelf;
            }
        }

        public void ToggleLayer(int index, bool isVisible)
        {
            if (index >= 0 && index < layers.Length)
            {
                layers[index].SetActive(isVisible);
                layerStates[index] = isVisible;

                // Only the owner of the Photon View should send updates
                if (photonView.IsMine)
                {
                    photonView.RPC("UpdateLayerState", RpcTarget.Others, index, isVisible);
                }
            }
        }

        [PunRPC]
        private void UpdateLayerState(int index, bool isVisible)
        {
            if (index >= 0 && index < layers.Length)
            {
                layers[index].SetActive(isVisible);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (layerStates == null) return;

            if (stream.IsWriting)
            {
                // Send the current layer states to other players
                foreach (bool state in layerStates)
                {
                    stream.SendNext(state);
                }
            }
            else
            {
                // Receive the layer states from other players
                for (int i = 0; i < layerStates.Length; i++)
                {
                    layerStates[i] = (bool)stream.ReceiveNext();
                    layers[i].SetActive(layerStates[i]);
                }
            }
        }
    }
}
