using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ClearView
{
    public class TransparencyEditor : MonoBehaviour
    {
        public Transform parentObject; // The parent object containing the child objects to adjust

        [Range(1, 100)]
        public float transparencyLevel = 100; // Transparency range from 1 (fully transparent) to 100 (fully opaque)

        [SerializeField] private List<Material> childMaterials;

        public void Setup(Transform model)
        {
            parentObject = model;
            // Get all the child objects' materials
            childMaterials = GetAllChildMaterials(parentObject);

            // Adjust transparency initially
            UpdateTransparency();
        }

        void Update()
        {
            if (parentObject != null)
            {
                // Adjust transparency in real-time (based on the inspector value or other runtime changes)
                UpdateTransparency();
            }
        }

        // Function to get the materials from all child objects
        public List<Material> GetAllChildMaterials(Transform model)
        {
            // Get all child renderers of the object
            Renderer[] childRenderers = model.GetComponentsInChildren<Renderer>();

            // Collect materials from all child renderers
            childMaterials = new List<Material>();

            foreach (var renderer in childRenderers)
            {
                var mat = renderer.sharedMaterial;
                childMaterials.Add(mat);
            }

            return childMaterials;
        }


        // Function to update transparency for all materials
        public void UpdateTransparency()
        {

            // Convert transparencyLevel (1-100) to alpha value (0.01 to 1.0)
            float alphaValue = Mathf.Clamp(transparencyLevel / 100f, 0.0f, 1f);

            // Loop through all child materials and update their alpha value
            foreach (Material mat in childMaterials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = alphaValue; // Set the alpha value
                    mat.color = color;
                }
            }

        }

        // Optionally call this method at runtime to set a new transparency level
        public void SetTransparencyLevel(float newTransparency)
        {
            transparencyLevel = Mathf.Clamp(newTransparency, 0, 100);
            UpdateTransparency();
        }
    }
}
