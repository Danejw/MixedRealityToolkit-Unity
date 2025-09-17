using UnityEngine;

namespace ClearView
{
    // This the highest level of the app, hold resources that are used by multiple scripts
    public class App : MonoBehaviour
    {
        // Singleton
        public static App Instance { get; private set; }

        // Resources
        public MicrosoftAuth MicrosoftAuth { get; private set; }
        public OneDriveManager OneDriveManager { get; private set; }
        public UIManager UIManager { get; private set; }

        public ModelManager ModelManager;

        private void Awake()
        {
            // Make Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Initialize
            // TODO: find in children recursively
            // ModelManager = GetComponentInChildren<ModelManager>();
            MicrosoftAuth = GetComponentInChildren<MicrosoftAuth>();
            OneDriveManager = GetComponentInChildren<OneDriveManager>();
            UIManager = GetComponentInChildren<UIManager>();
        }
    }
}
