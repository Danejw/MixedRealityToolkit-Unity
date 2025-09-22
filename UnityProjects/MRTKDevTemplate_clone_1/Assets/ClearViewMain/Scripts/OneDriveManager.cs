using GLTFast;
using GLTFast.Materials;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace ClearView
{
    public class GLBMetadata
    {
        public string Exporter;
        public string CreationDate;
        public string Author;
        public string Description;
        public bool Editable;

        // Constructor to set the metadata fields
        public GLBMetadata(string exporter, string author, string description, bool editable)
        {
            Exporter = exporter;
            CreationDate = DateTime.Now.ToString("yyyy-MM-dd");
            Author = author;
            Description = description;
            Editable = editable;
        }

        // Convert the metadata to a JObject
        public JObject ToJObject()
        {
            return new JObject
            {
                ["exporter"] = Exporter,
                ["creationDate"] = CreationDate,
                ["author"] = Author,
                ["description"] = Description,
                ["editable"] = Editable
            };
        }
    }

    // This is what we will use to communicate with the OneDrive API
    public class OneDriveManager : MonoBehaviour
    {
        private HttpClient httpClient;

        //public string fileName;
        //public string fileId;

        public string filePath;

        private float startTime;
        [SerializeField] private float importTime;


        public event Action<GltfImport, GameObject> OnImportComplete; // reutrns the instantiated object
        public event Action OnInitialize;

        private void Start()
        {
            App.Instance.MicrosoftAuth.OnAuthenticated += InitializeClient;
        }

        public void InitializeClient(string accessToken)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            Logger.Log(Logger.Category.Info, "OneDrive client initialized.");
            OnInitialize?.Invoke();
        }


        public async Task<Dictionary<string, string>> ListAllFilesInOneDrive()
        {
            var files = new Dictionary<string, string>();

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
                            //Debug.Log($"File: {file}");
                            files.Add(file["name"].ToString(), file["id"].ToString());
                        }

                        return files;
                    }
                    else
                    {
                        Logger.Log(Logger.Category.Info, "No files found in OneDrive.");

                        return files;
                    }
                }
                else
                {
                    Logger.Log(Logger.Category.Error, $"Failed to list files: {response.ReasonPhrase}");

                    return files;
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Log(Logger.Category.Error, $"HTTP Request error: {ex.Message}");
                return files;
            }
        }


        public async Task DownloadFromOneDrive(string fileName, string fileId)
        {
            // Get the file extension in lowercase
            string extension = Path.GetExtension(fileName).ToLower();

            // Switch based on file extension
            switch (extension)
            {
                case ".fbx":
                    await DownloadFBXFile(fileName, fileId);
                    break;
                case ".glb":
                case ".gltf":
                    await DownloadAndLoadGLTF(fileName, fileId);
                    break;

                default:
                    Debug.LogWarning($"File '{fileName}' is not a supported 3D model format.");
                    // Handle unsupported file formats here
                    break;
            }
        }


        // FBX

        private async Task DownloadFBXFile(string fileName, string fileId)
        {
            try
            {
                // TODO: Validate the file extension before downloading
                if (!fileName.EndsWith(".fbx"))
                {
                    Logger.Log(Logger.Category.Error, "Invalid file extension. Please select an FBX file.");
                    return;
                }

                startTime = Time.realtimeSinceStartup;

                // OneDrive API endpoint to get file content
                var url = $"https://graph.microsoft.com/v1.0/me/drive/items/{fileId}/content";

                // Make the request to download the file
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    Logger.Log(Logger.Category.Info, "Downloading FBX file...");

                    // Save the file to the persistent data path
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    filePath = Path.Combine(Application.dataPath, $"{fileName}");

                    File.WriteAllBytes(filePath, fileBytes);

                    Logger.Log(Logger.Category.Info, $"FBX file downloaded to: {filePath}");

                    // Import the FBX model
                    ImportFBX(filePath);

                    importTime = Time.realtimeSinceStartup - startTime;
                }
                else
                {
                    Logger.Log(Logger.Category.Error, $"Failed to download FBX file: {response.ReasonPhrase}");

                    importTime = Time.realtimeSinceStartup - startTime;
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Log(Logger.Category.Error, $"HTTP Request error: {ex.Message}");

                importTime = Time.realtimeSinceStartup - startTime;
            }
        }

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

            // Return the file path to the calling method
            //OnImportComplete?.Invoke(filePath);
        }


        // GLTF / GLB

        private async Task DownloadAndLoadGLTF(string fileName, string fileId)
        {
            try
            {
                // Ensure the file is a GLTF or GLB file
                if (!fileName.EndsWith(".gltf") && !fileName.EndsWith(".glb"))
                {
                    Logger.Log(Logger.Category.Error, "The specified file is not a GLTF or GLB file.");
                    return;
                }

                startTime = Time.realtimeSinceStartup;

                // OneDrive API endpoint to get file content
                var url = $"https://graph.microsoft.com/v1.0/me/drive/items/{fileId}/content";

                // Make the request to download the file
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    Logger.Log(Logger.Category.Info, "Downloading GLTF/GLB file...");

                    // Save the file to the persistent data path
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    filePath = Path.Combine(Application.persistentDataPath, fileName);

                    File.WriteAllBytes(filePath, fileBytes);

                    Logger.Log(Logger.Category.Info, $"GLTF/GLB file downloaded to: {filePath}");

                    // Load the GLTF model at runtime
                    await LoadGLTFAtRuntime(filePath);

                    // Extract and print metadata from the GLB file
                    //PrintGLBMetadata(filePath);

                    importTime = Time.realtimeSinceStartup - startTime;
                }
                else
                {
                    Logger.Log(Logger.Category.Error, $"Failed to download GLTF/GLB file: {response.ReasonPhrase}");

                    importTime = Time.realtimeSinceStartup - startTime;
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Log(Logger.Category.Error, $"HTTP Request error: {ex.Message}");

                importTime = Time.realtimeSinceStartup - startTime;
            }
        }

        private async Task<(GltfImport, GameObject)> LoadGLTFAtRuntime(string filePath)
        {
            // URP specific material setup
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            // Set up the material generator manually
            IMaterialGenerator materialGenerator = new UniversalRPMaterialGenerator(urpAsset);


            // Create a new GLTFast importer instance
            var gltfImport = new GltfImport(null, null, materialGenerator, null);

            // Load the GLTF file from the file path
            bool success = await gltfImport.Load(filePath);

            if (success)
            {
                // Instantiate the loaded GLTF model in the scene
                GameObject gltfObject = new GameObject(Path.GetFileName(filePath));
                //gltfImport.InstantiateMainScene(gltfObject.transform);
                Logger.Log(Logger.Category.Info, "GLTF/GLB file loaded and instantiated at runtime.");

                OnImportComplete?.Invoke(gltfImport, gltfObject);

                return (gltfImport, gltfObject);
            }
            else
            {
                Logger.Log(Logger.Category.Error, "Failed to load GLTF/GLB at runtime.");
                return default;
            }
        }

        public void PrintGLBMetadata()
        {
            try
            {
                // Read the GLB file as a byte array
                byte[] fileBytes = File.ReadAllBytes(filePath);

                // Check if the file is long enough to be a GLB file
                if (fileBytes.Length < 20)
                {
                    Logger.Log(Logger.Category.Error, "The file is too small to be a valid GLB file.");
                    return;
                }

                // Read the JSON chunk length from bytes 12-16
                int jsonLength = BitConverter.ToInt32(fileBytes, 12);

                // Read the chunk type to ensure it's JSON (should be "JSON" in ASCII)
                string chunkType = Encoding.ASCII.GetString(fileBytes, 16, 4);
                if (chunkType != "JSON")
                {
                    Logger.Log(Logger.Category.Error, "The first chunk is not of type 'JSON'.");
                    return;
                }

                // Extract the JSON content from the GLB file starting at byte 20
                string jsonContent = Encoding.UTF8.GetString(fileBytes, 20, jsonLength);

                // Print the extracted JSON content
                Logger.Log(Logger.Category.Info, $"GLB JSON Content: {jsonContent}");
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Category.Error, $"Failed to extract JSON from GLB: {ex.Message}");
            }
        }

        private void AddMetadataToGLBAsset(string inputPath, string outputPath, GLBMetadata metadata)
        {
            try
            {
                // Read the GLB file as a byte array
                byte[] fileBytes = File.ReadAllBytes(inputPath);

                // Read the original JSON chunk length from bytes 12-16
                int jsonLength = BitConverter.ToInt32(fileBytes, 12);

                // Ensure the chunk type is 'JSON'
                string chunkType = Encoding.ASCII.GetString(fileBytes, 16, 4);
                if (chunkType != "JSON")
                {
                    Logger.Log(Logger.Category.Error, "The first chunk is not of type 'JSON'.");
                    return;
                }

                // Extract and parse the original JSON content
                string originalJsonContent = Encoding.UTF8.GetString(fileBytes, 20, jsonLength);
                JObject gltfJson = JObject.Parse(originalJsonContent);

                // Add metadata to the 'asset' level under the 'extras' field
                if (gltfJson["asset"] == null)
                {
                    gltfJson["asset"] = new JObject();
                }

                if (gltfJson["asset"]["extras"] == null)
                {
                    gltfJson["asset"]["extras"] = new JObject();
                }

                // Add the provided metadata to the 'extras' field
                gltfJson["asset"]["extras"] = metadata.ToJObject();

                // Convert the modified JSON back to bytes
                byte[] modifiedJsonBytes = Encoding.UTF8.GetBytes(gltfJson.ToString());
                int modifiedJsonLength = modifiedJsonBytes.Length;

                // Calculate padding to make JSON chunk size 4-byte aligned
                int jsonPadding = (4 - (modifiedJsonLength % 4)) % 4;

                // Get the binary chunk from the original GLB
                int binaryChunkStart = 20 + jsonLength;
                byte[] binaryChunkBytes = new byte[fileBytes.Length - binaryChunkStart];
                Array.Copy(fileBytes, binaryChunkStart, binaryChunkBytes, 0, binaryChunkBytes.Length);

                // Calculate the new GLB length
                int newGLBLength = 12 + 8 + modifiedJsonLength + jsonPadding + binaryChunkBytes.Length;

                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    // Write the GLB header
                    fs.Write(Encoding.ASCII.GetBytes("glTF"), 0, 4); // Magic
                    fs.Write(BitConverter.GetBytes(2), 0, 4); // Version
                    fs.Write(BitConverter.GetBytes(newGLBLength), 0, 4); // Total length

                    // Write the JSON chunk header
                    fs.Write(BitConverter.GetBytes(modifiedJsonLength + jsonPadding), 0, 4); // Chunk length
                    fs.Write(Encoding.ASCII.GetBytes("JSON"), 0, 4); // Chunk type

                    // Write the modified JSON content
                    fs.Write(modifiedJsonBytes, 0, modifiedJsonLength);

                    // Write the JSON padding
                    for (int i = 0; i < jsonPadding; i++)
                    {
                        fs.WriteByte(0x20); // Space character
                    }

                    // Write the binary chunk (if any)
                    if (binaryChunkBytes.Length > 0)
                    {
                        // Write the binary chunk header
                        fs.Write(BitConverter.GetBytes(binaryChunkBytes.Length), 0, 4); // Chunk length
                        fs.Write(Encoding.ASCII.GetBytes("BIN\0"), 0, 4); // Chunk type

                        // Write the binary chunk data
                        fs.Write(binaryChunkBytes, 0, binaryChunkBytes.Length);
                    }
                }

                Logger.Log(Logger.Category.Info, $"Metadata added and GLB recreated. Saved as: {outputPath}");
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Category.Error, $"Failed to add metadata to GLB: {ex.Message}");
            }
        }

        private void AddMetadataToGLBAsset()
        {
            // Create metadata object
            var metadata = new GLBMetadata(
                exporter: "Custom Exporter",
                author: "Dane",
                description: "GLB file with expanded metadata",
                editable: true
            );

            // Add metadata to the GLB asset level
            AddMetadataToGLBAsset(filePath, filePath, metadata);
        }
    }
}

