using Photon.Pun;
using UnityEngine;


namespace ClearView.Network
{
    // This script scynchronizes the position and rotation of the clipping tool over the network as they are changed by the owner
    public class ClippingToolSync : MonoBehaviourPun, IPunObservable
    {
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (!this.gameObject.activeSelf) return; // do nothing if not active

            if (stream.IsWriting)
            {
                // Send the current value to other players
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                // Receive the value from other players
                stream.ReceiveNext();
            }
        }
    }
}
