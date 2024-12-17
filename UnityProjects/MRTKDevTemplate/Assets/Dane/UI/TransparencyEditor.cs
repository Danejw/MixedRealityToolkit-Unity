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
        [SerializeField] private ShaderRenderingModeSwitcher switcher;

        public void Setup(Transform model)
        {
            parentObject = model;

            this.switcher = null;

            // Adjust transparency initially
            UpdateTransparency();

            bool mod = parentObject.TryGetComponent(out ShaderRenderingModeSwitcher switcher);
            if (mod)
            {
                Debug.Log("ShaderRenderingModeSwitcher found");
            }
            else
            {
                Debug.Log("ShaderRenderingModeSwitcher not found");
            }
        }

        void Update()
        {
            if (parentObject != null)
            {
                // Adjust transparency in real-time (based on the inspector value or other runtime changes)
                UpdateTransparency();
            }


        }

        // Function to update transparency for all materials
        public void UpdateTransparency()
        {
            // Convert transparencyLevel (1-100) to alpha value (0.01 to 1.0)
            //float alphaValue = Mathf.Clamp(transparencyLevel, 0.0f, 1f);

            if (!switcher) parentObject.TryGetComponent<ShaderRenderingModeSwitcher>(out switcher);

            //remap transparency level to alpha value
            float alphaValue = transparencyLevel / 100;

            if (switcher) switcher.SetAlpha(alphaValue);
        }

        // Optionally call this method at runtime to set a new transparency level
        public void SetTransparencyLevel(float newTransparency)
        {
            transparencyLevel = Mathf.Clamp(newTransparency, 0, 100);
            UpdateTransparency();
        }
    }
}
