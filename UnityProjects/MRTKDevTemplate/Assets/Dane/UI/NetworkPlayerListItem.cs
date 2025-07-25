using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace ClearView.UI
{
    public class NetworkPlayerListItem : MonoBehaviour
    {
        public TMP_Text text;

        public GameObject hostIcon;

        public RawImage backplate;
        public int localTransparencyLevel = 125; // 0-255
        public int nonlocalTransparencyLevel = 75; // 0-255

        public void SetPlayerInfo(string playerName, bool isHost, bool isLocalPlayer)
        {
            text.text = playerName;
            hostIcon?.SetActive(isHost);

            // Adjust backplate transparency based on local status
            Color currentColor = backplate.color;
            float alpha = isLocalPlayer ? localTransparencyLevel / 255f : nonlocalTransparencyLevel / 255f;
            backplate.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }
}
