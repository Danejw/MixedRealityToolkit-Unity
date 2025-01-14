using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ClearView.Network
{
    // This script scynchronizes the clipping tool posiiont and orientation over the network as they are changed by the owner
    public class ClippingToolSync : MonoBehaviourPun, IPunObservable
    {
        public GameObject clippingTool;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (!clippingTool) return;

            if (stream.IsWriting)
            {
                // Send the current layer states to other players
                stream.SendNext(clippingTool.transform.position);
                stream.SendNext(clippingTool.transform.rotation);
            }
            else
            {
                stream.ReceiveNext();
            }
        }
    }
}
