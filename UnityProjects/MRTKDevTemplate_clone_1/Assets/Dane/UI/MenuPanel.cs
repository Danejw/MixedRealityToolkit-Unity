using UnityEngine;


namespace ClearView
{
    public class MenuPanel : MonoBehaviour
    {
        public bool onAtStart = false;

        private void Start()
        {
            gameObject.SetActive(onAtStart);
        }
    }
}
