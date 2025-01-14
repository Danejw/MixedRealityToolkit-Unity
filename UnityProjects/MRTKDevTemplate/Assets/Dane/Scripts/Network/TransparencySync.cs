using Photon.Pun;
using UnityEngine;


namespace ClearView.Network
{
    // This script scynchronizes the transparency of the model layers over the network as they are changed by the owner
    public class TransparencySync : MonoBehaviourPun, IPunObservable
    {
        public TransparencyEditor tranparencyEditor;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (!tranparencyEditor) return;

            if (stream.IsWriting)
            {
                // Send the current layer states to other players
                 stream.SendNext(tranparencyEditor.transparencyLevel);
            }
            else
            {
                // Receive the layer states from other players
                stream.ReceiveNext();
            }
        }
    }
}
