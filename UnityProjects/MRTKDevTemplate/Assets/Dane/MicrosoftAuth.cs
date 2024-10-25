using System.Linq;
using UnityEngine;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using System;
using dotenv.net;
using UnityEditor.PackageManager;



namespace ClearView
{
    public class MicrosoftAuth : MonoBehaviour
    {
        public static event Action<string> OnAuthenticated;
        public static event Action OnSignOut;


        private string clientId; // store this securely
        private const string Authority = "https://login.microsoftonline.com/common";


        private IPublicClientApplication app;

        [SerializeField] private IAccount account;

        [SerializeField] private string accessToken;
        public string AccessToken
        {
            get => accessToken;
            private set
            {
                accessToken = value;
                OnAuthenticated?.Invoke(value);
            }
        }


        private void Start()
        {
            // Load .env variables
            LoadEnvironmentVariable();

            app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(Authority)
                .WithRedirectUri("http://localhost") // Change this to your redirect URI
                .Build();
        }

        public void LoadEnvironmentVariable()
        {
            try
            {
                // Load .env file in the Unity project directory
                DotEnv.Load();
                clientId = Environment.GetEnvironmentVariable("AZURE_APP_CLIENT_ID");

                if (string.IsNullOrEmpty(clientId))
                {
                    Debug.LogError("Client ID is missing. Please set AZURE_APP_CLIENT_ID in your .env file.");
                }
                else
                {
                    Debug.Log($"Client ID loaded");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load .env file: {ex.Message}");
            }
        }


        public async Task SignIn()
        {
            var scopes = new[] { "User.Read", "Files.Read.All", "Files.Read", "Files.ReadWrite", "Files.ReadWrite.All" }; // Permissions for OneDrive

            // Attempt to sign in silently at first, if that fails, sign in interactively
            AuthenticationResult result = null;
            try
            {
                Debug.Log($"Attempting to signin silently...");

                var accounts = await app.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();
                result = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync();
            }
            catch (MsalException ex)
            {
                try
                {
                    Debug.Log($"Attempting to signin interactively...");

                    result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
                }
                catch (MsalUiRequiredException x)
                {
                    Debug.LogError($"Sign-In Failed: {x.Message}");
                }
            }
            finally
            {
                Debug.Log($"Sign-In Complete.");

                if (result != null)
                {
                    AccessToken = result.AccessToken;
                    account = result.Account;

                    Debug.Log($"Access Token: {result.AccessToken}");

                    // Display user info
                    if (account != null)
                    {
                        Debug.Log($"User Name: {account.Username}");
                        Debug.Log($"User ID: {account.HomeAccountId}");
                    }
                }
            }
        }


        public async Task SignOut()
        {
            try
            {
                if (account != null)
                {
                    // Remove the account from the token cache
                    await app.RemoveAsync(account);
                    Debug.Log("User signed out successfully.");

                    OnSignOut?.Invoke();
                }
                else
                {
                    Debug.Log("No user is currently signed in.");
                }
            }
            catch (MsalException ex)
            {
                Debug.LogError($"Sign-out failed: {ex.Message}");
            }
        }
    }
}
