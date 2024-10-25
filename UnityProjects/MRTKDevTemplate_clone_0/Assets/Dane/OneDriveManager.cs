using GLTFast;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;

namespace ClearView
{
    public class OneDriveManager : MonoBehaviour
    {
        private HttpClient httpClient;

        public string fileName;
        public string fileId;

        private void Start()
        {
            MicrosoftAuth.OnAuthenticated += InitializeClient;
        }

        public void InitializeClient(string accessToken)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            Debug.Log("OneDrive client initialized.");
        }


        public async Task ListAllFilesInOneDrive()
        {
            try
            {
                string url = "https://graph.microsoft.com/v1.0/me/drive/root/children";
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResult = await response.Content.ReadAsStringAsync();
                    JObject filesData = JObject.Parse(jsonResult);

                    // Parse and list file details
                    if (filesData["value"] != null)
                    {
                        foreach (var file in filesData["value"])
                        {
                            string fileName = file["name"].ToString();
                            string fileId = file["id"].ToString();
                            Debug.Log($"File: {fileName}, ID: {fileId}");
                        }
                    }
                    else
                    {
                        Debug.Log("No files found in OneDrive.");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to list files: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.LogError($"HTTP Request error: {ex.Message}");
            }
        }


        // FBX

        public async Task DownloadFBXFile()
        {

            // TODO: Validate the file extension before downloading
            if (!this.fileName.EndsWith(".fbx"))
            {
                Debug.LogError("Invalid file extension. Please select an FBX file.");
                return;
            }


            try
            {
                // OneDrive API endpoint to get file content
                var url = $"https://graph.microsoft.com/v1.0/me/drive/items/{this.fileId}/content";

                // Make the request to download the file
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Downloading FBX file...");

                    // Save the file to the persistent data path
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    string filePath = Path.Combine(Application.dataPath, $"{this.fileName}");

                    File.WriteAllBytes(filePath, fileBytes);

                    Debug.Log($"FBX file downloaded to: {filePath}");

                    // Import the FBX model
                    ImportFBX(filePath);
                }
                else
                {
                    Debug.LogError($"Failed to download FBX file: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.LogError($"HTTP Request error: {ex.Message}");
            }
        }

        /*
        // TODO: Import UnityFBXImporter or similar library
        private void LoadFBXAtRuntime(string filePath)
        {
            // Load the FBX file at runtime using UnityFBXImporter or a similar library
            var fbxObject = FBXImporter.Load(filePath);

            if (fbxObject != null)
            {
                Instantiate(fbxObject);
                Debug.Log("FBX file loaded and instantiated at runtime.");
            }
            else
            {
                Debug.LogError("Failed to load FBX at runtime.");
            }
        }
        */

        private void ImportFBX(string filePath)
        {
            // Load the FBX file into Unity (Editor-only)
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.ImportAsset(filePath);
            Debug.Log("FBX file imported into Unity.");
            UnityEditor.AssetDatabase.Refresh();
#else
            Debug.LogWarning("FBX importing works only in the Unity Editor.");
#endif
        }


        // GLTF

        public async Task DownloadAndLoadGLTF()
        {
            try
            {
                // Ensure the file is a GLTF or GLB file
                if (!fileName.EndsWith(".gltf") && !fileName.EndsWith(".glb"))
                {
                    Debug.LogError("The specified file is not a GLTF or GLTF/GLB file.");
                    return;
                }

                // OneDrive API endpoint to get file content
                var url = $"https://graph.microsoft.com/v1.0/me/drive/items/{fileId}/content";

                // Make the request to download the file
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Downloading GLTF/GLB file...");

                    // Save the file to the persistent data path
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    string filePath = Path.Combine(Application.persistentDataPath, fileName);

                    File.WriteAllBytes(filePath, fileBytes);

                    Debug.Log($"GLTF/GLB file downloaded to: {filePath}");

                    // Load the GLTF model at runtime
                    LoadGLTFAtRuntime(filePath);
                }
                else
                {
                    Debug.LogError($"Failed to download GLTF/GLB file: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.LogError($"HTTP Request error: {ex.Message}");
            }
        }

        private async void LoadGLTFAtRuntime(string filePath)
        {
            // Create a new GLTFast importer instance
            var gltfImport = new GltfImport();

            // Load the GLTF file from the file path
            bool success = await gltfImport.Load(filePath);

            if (success)
            {
                // Instantiate the loaded GLTF model in the scene
                GameObject gltfObject = new GameObject(fileName);
                gltfImport.InstantiateMainScene(gltfObject.transform);
                Debug.Log("GLTF/GLB file loaded and instantiated at runtime.");
            }
            else
            {
                Debug.LogError("Failed to load GLTF/GLB at runtime.");
            }
        }
    }
}

