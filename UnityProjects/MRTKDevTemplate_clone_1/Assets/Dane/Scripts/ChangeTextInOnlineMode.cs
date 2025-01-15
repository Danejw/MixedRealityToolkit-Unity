using TMPro;
using UnityEngine;
using Photon.Pun;

namespace ClearView
{
    // Changes the text of a TMP_Text component based on whether the game is in online or offline mode
    public class ChangeTextInOnlineMode : MonoBehaviour
    {
        public TMP_Text text;

        public string offlineText = "Offline";
        public string onlineText = "Online";

        [SerializeField] private bool isOnline => PhotonNetwork.InRoom;

        private void LateUpdate()
        {
            if (text != null && text.text != (isOnline ? onlineText : offlineText))
                text.text = isOnline ? onlineText : offlineText;
        }
    }
}
