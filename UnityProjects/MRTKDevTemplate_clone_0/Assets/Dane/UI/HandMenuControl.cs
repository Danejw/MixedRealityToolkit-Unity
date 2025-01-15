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

            if (photonView.IsMine)
            {
                // Enable the hand menu
                gameObject.SetActive(true);
            }
            else
            {
                // Disable the hand menu
                gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (!photonView) photonView = GetComponent<PhotonView>();
        }
    }
}
