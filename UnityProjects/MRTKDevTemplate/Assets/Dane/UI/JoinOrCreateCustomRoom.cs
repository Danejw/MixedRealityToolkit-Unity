using ClearView.Network;
using TMPro;
using UnityEngine;


namespace ClearView
{
    public class JoinOrCreateCustomRoom : MonoBehaviour
    {
        public TMP_InputField roomNameInput;

        public void JoinOrCreateRoom()
        {
            // Sanity check
            if (string.IsNullOrEmpty(roomNameInput.text))
            {
                Debug.LogError("Room name is null or empty");
                return;
            }

            NetworkManager.Instance.JoinOrCreateRoom();//roomNameInput.text);
        }
    }
}
