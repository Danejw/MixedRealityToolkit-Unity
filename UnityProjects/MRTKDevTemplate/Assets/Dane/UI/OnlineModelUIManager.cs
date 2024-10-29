using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


namespace ClearView.UI
{
    public class OnlineModelUIManager : MonoBehaviour
    {
        public ModelManager modelManager;

        public GameObject itemPrefab;


        public List<GameObject> items = new List<GameObject>();

        private void Start()
        {
            modelManager.OnlineModelsUpdated += UpdateOnlineModels;
        }

        private void OnEnable()
        {
            modelManager?.InitOneDrive();
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
            foreach (var item in modelManager.OnlineModels)
            {
                var go = Instantiate(itemPrefab, transform);
                go.name = item.Key;

                go.TryGetComponent<OnlineModelUIItem>(out var onlineModelUIItem);
                onlineModelUIItem?.SetText(item.Key);

                go.TryGetComponent<PressableButton>(out var pb);

                pb?.selectEntered.AddListener((SelectEnterEventArgs args) =>
                {
                    modelManager?.ImportModel(item.Key);
                });


                items.Add(go);
            }
        }
    }
}
