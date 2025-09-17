using System.Linq;
using UnityEngine;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using System;
using dotenv.net;
using Azure.Core;


namespace ClearView
{
    // This is what we will use to authenticate with Microsoft Graph
    public class MicrosoftAuth : MonoBehaviour
    {
        public event Action<string> OnAuthenticated;
        public event Action OnSignOut;

        [SerializeField] private bool autoSignIn = true;
        private string token;

        private string clientId; // store this securely
        private const string Authority = "https://login.microsoftonline.com/common";

        public enum State
        {
            NotAuthenticated,
            Authenticated,
        }

        public State currentState = State.NotAuthenticated;

        private IPublicClientApplication app;

        [SerializeField] private IAccount account;

        [SerializeField] private string accessToken;
        public string AccessToken
        {
            get => accessToken;
            private set
            {
                accessToken = value;
                OnAuthenticated?.Invoke(accessToken);
                currentState = State.Authenticated;
            }
        }


        private async void Start()
        {
            // Load .env variables
            LoadEnvironmentVariable();

            app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(Authority)
                .WithRedirectUri("http://localhost") // Change this to your redirect URI
                .Build();


            if (autoSignIn)
            {
                // Check if access token is stored in player prefs
                if (PlayerPrefs.HasKey("AccessToken"))
                {
                    // Load access token from player prefs
                    AccessToken = PlayerPrefs.GetString("AccessToken");
                }
                else
                {
                    // Sign in
                    await SignIn();
                }
            }
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
                    Logger.Log(Logger.Category.Error, "Client ID is missing. Please set AZURE_APP_CLIENT_ID in your .env file.");
                }
                else
                {
                    Logger.Log(Logger.Category.Info, "Client ID loaded");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Category.Error, $"Failed to load .env file: {ex.Message}");
            }
        }

        public async void DoSignIn()
        {
            await SignIn();
        }

        public async Task SignIn()
        {
            var scopes = new[] { "User.Read", "Files.Read.All", "Files.Read", "Files.ReadWrite", "Files.ReadWrite.All" }; // Permissions for OneDrive

            // Attempt to sign in silently at first, if that fails, sign in interactively
            AuthenticationResult result = null;
            try
            {
                Logger.Log(Logger.Category.Info, "Attempting to sign in silently...");

                var accounts = await app.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();
                result = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync();
            }
            catch (MsalException ex)
            {
                try
                {
                    Logger.Log(Logger.Category.Warning, "Attempting to sign in interactively...");

                    Logger.Log(Logger.Category.Error, $"Sign-In Failed: {ex.Message}");
                }
                catch (MsalUiRequiredException x)
                {
                    Debug.LogError($"Sign-In Failed: {x.Message}");
                }
            }
            finally
            {
                Logger.Log(Logger.Category.Info, "Sign-In Complete.");

                if (result != null)
                {
                    AccessToken = result.AccessToken;
                    account = result.Account;

                    Logger.Log(Logger.Category.Info, $"Access Token: {result.AccessToken}");

                    // Store access token in player prefs
                    PlayerPrefs.SetString("AccessToken", result.AccessToken);

                    // Display user info
                    if (account != null)
                    {
                        Logger.Log(Logger.Category.Info, $"User Name: {account.Username}");
                        Logger.Log(Logger.Category.Info, $"User ID: {account.HomeAccountId}");
                    }
                }
            }
        }

        public async void DoSignOut()
        {
            await SignOut();
        }

        public async Task SignOut()
        {
            try
            {
                if (account != null)
                {
                    // Remove the account from the token cache
                    await app.RemoveAsync(account);
                    Logger.Log(Logger.Category.Info, "User signed out successfully.");

                    // Clear access token from player prefs
                    PlayerPrefs.DeleteKey("AccessToken");

                    currentState = State.NotAuthenticated;

                    OnSignOut?.Invoke();
                }
                else
                {
                    Logger.Log(Logger.Category.Warning, "No user is currently signed in.");
                }
            }
            catch (MsalException ex)
            {
                Logger.Log(Logger.Category.Error, $"Sign-out failed: {ex.Message}");
            }
        }

        public string GetUsername()
        {
            if (currentState == State.NotAuthenticated) return "";
            return account.Username;
        }
    }
}
