using ClearView.Network;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClearView
{
    public class CustomToggleCollection : MonoBehaviour
    {
        public Transform verticalLayout;
        public CustomToggle prefab;

        [SerializeField]
        [Tooltip("Array of StatefulInteractable toggles that will be managed by this controller.")]
        public List<CustomToggle> toggles = new List<CustomToggle>();

        private ModelLayerSync modelLayerSync;

        public void SetToggleCollection(Transform model)
        {
            RemoveCurrentToggles(); // Clear existing toggles before creating new ones

            // Ensure the ModelLayerSync component exists on the root object
            if (model.TryGetComponent(out ModelLayerSync sync))
            {
                modelLayerSync = sync;
            }
            else
            {
                Debug.LogError($"ModelLayerSync component not found on {model.name}. Ensure it's attached to the root object.");
                return;
            }

            AddTogglesRecursively(model);
        }

        private void AddTogglesRecursively(Transform parent)
        {
            // Loop through each child of the current parent
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                // Create a new toggle for the child
                CustomToggle layerToggle = Instantiate(prefab, verticalLayout);

                // Set the toggle label to the child's name
                layerToggle.label.text = child.name;

                // Set the toggle initially based on the child's active state
                layerToggle.toggle.ForceSetToggled(child.gameObject.activeSelf);

                // Add a listener to the toggle to set the child's active state when toggled
                int childIndex = child.GetSiblingIndex(); // Capture sibling index for the toggle
                layerToggle.toggle.OnClicked.AddListener(() => OnToggleValueChanged(childIndex, child));

                // Add the new toggle to the list of toggles
                toggles.Add(layerToggle);

                // Recursively call this method for any nested children
                AddTogglesRecursively(child);
            }
        }

        // Listener function to handle the toggling of a child
        private void OnToggleValueChanged(int childIndex, Transform child)
        {
            // Find the toggle associated with this child
            CustomToggle correspondingToggle = toggles.FirstOrDefault(t => t.label.text == child.name);

            if (correspondingToggle != null && correspondingToggle.toggle != null)
            {
                // Get the toggle state
                bool isToggled = correspondingToggle.toggle.IsToggled;

                // Use ModelLayerSync to synchronize the toggle state across the network
                if (modelLayerSync != null)
                {
                    modelLayerSync.ToggleLayer(childIndex, isToggled);
                }
                else
                {
                    Debug.LogError("ModelLayerSync is not initialized. Ensure it's attached to the root model.");
                }
            }
        }

        public void RemoveCurrentToggles()
        {
            // Clear existing toggles
            if (toggles.Count > 0)
            {
                foreach (var toggle in toggles)
                {
                    Destroy(toggle.gameObject);
                }

                toggles.Clear();
            }
        }
    }
}
