using UnityEngine;

namespace ClearView
{
    // This the highest level of the app, hold resources that are used by multiple scripts
    public class App : MonoBehaviour
    {
        // Singleton
        public static App Instance { get; private set; }

        public GameObject SignInFlow;
        public GameObject MainAppUI;

        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Initialize
            ModelManager = GetComponentInChildren<ModelManager>();
            MicrosoftAuth = GetComponentInChildren<MicrosoftAuth>();
            OneDriveManager = GetComponentInChildren<OneDriveManager>();
        }

        private void Start()
        {
            SetAuthState(false);

            MicrosoftAuth.OnAuthenticated += (string token) => { SetAuthState(true); };
            MicrosoftAuth.OnSignOut += () => { SetAuthState(false); };
        }

        private void SetAuthState(bool state)
        {
            switch (state)
            {
                case false:
                    SignInFlow.SetActive(true);
                    MainAppUI.SetActive(false);
                    break;
                case true:
                    SignInFlow.SetActive(false);
                    MainAppUI.SetActive(true);
                    break;
            }
        }

        public ModelManager ModelManager { get; private set; }
        public MicrosoftAuth MicrosoftAuth { get; private set; }
        public OneDriveManager OneDriveManager { get; private set; }
    }
}
