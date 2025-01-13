using Photon.Pun;
using UnityEngine;


namespace ClearView
{
    public class HandMenuControl : MonoBehaviour
    {
        private PhotonView photonView;

        private void OnEnable()
        {
            if (!photonView) photonView = GetComponent<PhotonView>();
        }

        private void Start()
        {
            if (!photonView) photonView = GetComponent<PhotonView>();
        }
    }
}
