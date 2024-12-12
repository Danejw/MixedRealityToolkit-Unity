using UnityEngine;

namespace ClearView
{
    public class MeshClipper : MonoBehaviour
    {
        public Transform clippingObject; // The mesh to be used as the clipping region
        public Material targetMaterial; // Reference to the material using the clipping shader

        void Update()
        {
            if (clippingObject != null && targetMaterial != null)
            {
                // Update the material properties
                targetMaterial.SetVector("_Position", clippingObject.position);
            }
        }
    }
}