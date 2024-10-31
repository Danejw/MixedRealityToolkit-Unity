using Photon.Pun;
using UnityEngine;


namespace ClearView
{
    public class NetworkedPlayer : MonoBehaviour
    {
        public PhotonView photonView;

        [Space]
        [Header(("Visual"))]
        public GameObject headPrefab;
        public GameObject leftHandPrefab;
        public GameObject rightHandPrefab;

        [Space]
        [Header(("Target"))]
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        private void OnEnable()
        {
            if (!photonView.IsMine) return;

            // Find a special game object, there should only be one
            var rigConnector = FindAnyObjectByType<NetworkRigConnector>();
            head = rigConnector.head;
            leftHand = rigConnector.leftHand;
            rightHand = rigConnector.rightHand;
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            if (head == null) return;
            if (leftHand == null) return;
            if (rightHand == null) return;

            MapPosition(headPrefab.transform, head);
            MapPosition(leftHandPrefab.transform, leftHand);
            MapPosition(rightHandPrefab.transform, rightHand);
        }

        public void MapPosition(Transform fromTarget, Transform toTarget)
        {
            fromTarget.position = toTarget.position;
            fromTarget.rotation = toTarget.rotation;
        }
    }
}
