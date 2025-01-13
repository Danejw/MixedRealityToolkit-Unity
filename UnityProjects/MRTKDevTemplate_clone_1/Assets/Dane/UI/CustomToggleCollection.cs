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

        public void SetToggleCollection(Transform model)
        {
            RemoveCurrentToggles(); // Clear existing toggles before creating new ones


            AddTogglesRecursively(model);

            /* Add toggles for the first level of children only
            // Create toggles
            for (int i = 0; i < model.childCount; i++)
            {
                CustomToggle layerToggle = Instantiate(prefab, verticalLayout);

                Transform child = model.GetChild(i); // Get the current child

                // Set the toggle label to the child's name
                layerToggle.label.text = child.name;

                // Set the toggle initially based on the child's active state
                layerToggle.toggle.ForceSetToggled(child.gameObject.activeSelf);

                // Add a listener to the toggle to set the child's active state when toggled
                int childIndex = i; // Capture index in a local variable to avoid closure issues
                layerToggle.toggle.OnClicked.AddListener(() => OnToggleValueChanged(childIndex, model));

                toggles.Add(layerToggle);
            }
            */
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
                layerToggle.toggle.OnClicked.AddListener(() => OnToggleValueChanged(child));

                // Add the new toggle to the list of toggles
                toggles.Add(layerToggle);

                // Recursively call this method for any nested children
                AddTogglesRecursively(child);
            }
        }

        // Listener function to handle the toggling of a child
        private void OnToggleValueChanged(Transform child)
        {
            // Find the toggle associated with this child
            CustomToggle correspondingToggle = toggles.FirstOrDefault(t => t.label.text == child.name);

            // Check if the toggle was found and if it has a valid IsToggled property
            if (correspondingToggle != null && correspondingToggle.toggle != null)
            {
                // Get the toggle state and set the child active or inactive based on that
                bool isToggled = correspondingToggle.toggle.IsToggled; // This assumes StatefulInteractable has an IsToggled property

                // Set the child's active state based on the toggle's state
                child.gameObject.SetActive(isToggled);
            }
        }

        public void RemoveCurrentToggles()
        {
            // Clear existing layers
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
