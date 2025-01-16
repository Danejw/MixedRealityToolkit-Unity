using UnityEngine;

namespace ClearView
{
    // This is where we will manage the high level UI state
    public class UIManager : MonoBehaviour
    {
        public GameObject SignInFlow;
        public GameObject MainAppUI;


        private void Start()
        {
            // TODO: set back to false at start
            SetAuthState(true);

            // Sets the auth state based on the auth events

            // Bypass auth for now
            //App.Instance.MicrosoftAuth.OnAuthenticated += (string token) => { SetAuthState(true); };
            //App.Instance.MicrosoftAuth.OnSignOut += () => { SetAuthState(false); };
        }

        private void SetAuthState(bool state)
        {
            switch (state)
            {
                case false:
                    SignInFlow?.SetActive(true);
                    MainAppUI.SetActive(false);
                    break;
                case true:
                    SignInFlow?.SetActive(false);
                    MainAppUI.SetActive(true);
                    break;
            }
        }
    }
}