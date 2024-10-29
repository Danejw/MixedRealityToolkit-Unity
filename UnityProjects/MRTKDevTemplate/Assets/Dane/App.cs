using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace ClearView
{
    // This the highest level of the app, hold resources that are used by multiple scripts
    public class App : MonoBehaviour
    {
        // Singleton
        public static App Instance { get; private set; }


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

        public ModelManager ModelManager { get; private set; }
        public MicrosoftAuth MicrosoftAuth { get; private set; }
        public OneDriveManager OneDriveManager { get; private set; }
    }
}
