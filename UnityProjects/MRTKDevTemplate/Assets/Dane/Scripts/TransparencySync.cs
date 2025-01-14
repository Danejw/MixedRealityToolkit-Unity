using Photon.Pun;
using UnityEngine;


namespace ClearView.Network
{
    // This script scynchronizes the transparency of the model layers over the network as they are changed by the owner
    [RequireComponent(typeof(TransparencyEditor))]
    public class TransparencySync : MonoBehaviourPun, IPunObservable
    {
        public TransparencyEditor tranparencyEditor;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (!tranparencyEditor) tranparencyEditor = GetComponent<TransparencyEditor>();
            if (!tranparencyEditor) return;

            if (stream.IsWriting)
            {
                // Send the current value to other players
                stream.SendNext(tranparencyEditor.transparencyLevel);
            }
            else
            {
                // Receive the value from other players
                stream.ReceiveNext();
            }
        }
    }
}
