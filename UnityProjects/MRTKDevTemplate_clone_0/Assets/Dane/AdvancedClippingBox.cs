using UnityEngine;
using System.Collections.Generic;
using Microsoft.MixedReality.GraphicsTools;

namespace ClearView
{
    public class AdvancedClippingBox : ClippingBox
    {
        // List to store renderers
        public List<Renderer> renderers = new List<Renderer>();

        // Method to add a single renderer
        public void AddRenderer(Renderer renderer)
        {
            if (renderer != null && !renderers.Contains(renderer))
            {
                renderers.Add(renderer);
                UpdateShaderProperties(renderer.material);
            }
        }

        // Method to add multiple renderers
        public void AddRenderers(IEnumerable<Renderer> renderersToAdd)
        {
            foreach (var renderer in renderersToAdd)
            {
                AddRenderer(renderer);
            }
        }

        // Override to update shader properties for all renderers
        protected override void UpdateShaderProperties(Material material)
        {
            base.UpdateShaderProperties(material);
            foreach (var renderer in renderers)
            {
                renderer.material.SetMatrix(clipBoxInverseTransformID, clipBoxInverseTransform);
            }
        }
    }
}