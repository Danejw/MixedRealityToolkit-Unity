using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


namespace ClearView.UI
{
    // This is the portion of the UI that will manage/display UI elements representing the models available from a user's OneDrive
    public class OnlineModelUIManager : MonoBehaviour
    {
        public GameObject itemPrefab;
        public List<GameObject> items = new List<GameObject>();

        private void Start()
        {
            modelManager.OnlineModelsUpdated += UpdateOnlineModels;
            App.Instance.OneDriveManager.OnInitialize += OnOneDriveInit;
        }

        private void OnEnable()
        {
            if (App.Instance.MicrosoftAuth.currentState != MicrosoftAuth.State.Authenticated) return;

            App.Instance.ModelManager?.GetOneDriveFiles();
        }

        // Event
        private void OnOneDriveInit()
        {
            App.Instance.ModelManager?.GetOneDriveFiles();
        }

        private void UpdateOnlineModels(Dictionary<string, string> dictionary)
        {
            RemoveItems();

            AddItems();
        }

        private void RemoveItems()
        {
            foreach (var item in items)
            {
                Destroy(item);
            }
            items.Clear();
        }

        private void AddItems()
        {
            foreach (var item in App.Instance.ModelManager.OnlineModels)
            {
                var go = Instantiate(itemPrefab, transform);
                go.name = item.Key;

                go.TryGetComponent<OnlineModelUIItem>(out var onlineModelUIItem);
                onlineModelUIItem?.SetText(item.Key);

                go.TryGetComponent<PressableButton>(out var pb);

                pb?.selectEntered.AddListener((SelectEnterEventArgs args) =>
                {
                    App.Instance.ModelManager?.ImportModel(item.Key);
                });


                items.Add(go);
            }
        }
    }
}
