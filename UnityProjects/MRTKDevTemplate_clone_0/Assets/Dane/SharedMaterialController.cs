using UnityEngine;
using System.Collections.Generic;

namespace ClearView
{
    public class SharedMaterialController : MonoBehaviour
    {
        [SerializeField] protected List<Renderer> targetRenderers = new List<Renderer>(); // List of child renderers

        public string transparencyProperty = "_Transparency"; // Shader property names

        public string xProperty = "_X";
        public string yProperty = "_Y";
        public string zProperty = "_Z";

        public string spreadProperty = "_Spread";



        [Range(0, 1)]
        public float transparency = 0.25f;

        [Range(-1.5f, 1.5f)]
        public float x = 1.5f;

        [Range(-1.5f, 1.5f)]
        public float y = 1.5f;

        [Range(-1.5f, 1.5f)]
        public float z = 1.5f;

        [Range(0.01f, 1f)]
        public float spread = 0.01f;


        protected virtual void Start()
        {
            // Gather all MeshRenderers from children
            if (targetRenderers.Count == 0)
            {
                GetChildRenderers();
            }
        }

        private void GetChildRenderers()
        {
            targetRenderers.Clear();
            MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();
            if (childRenderers.Length == 0)
            {
                Debug.LogError("No child MeshRenderers found!");
                return;
            }

            foreach (var renderer in childRenderers)
            {
                if (renderer.sharedMaterial != null)
                {
                    targetRenderers.Add(renderer);
                }
                else
                {
                    Debug.LogWarning($"Renderer on {renderer.gameObject.name} has no shared material.");
                }
            }
        }

        private void Update()
        {
            // If transparency reaches 0, set x, y, z to their minimum values
            if (transparency <= 0)
            {
                x = -1.5f;
                y = -1.5f;
                z = -1.5f;
            }

            // If transparency reaches 1, set x, y, z to their maximum values
            if (transparency >= 1)
            {
                x = 1.5f;
                y = 1.5f;
                z = 1.5f;
            }

            // Update shader properties for all child renderers
            foreach (var renderer in targetRenderers)
            {
                Material sharedMaterial = renderer.sharedMaterial;
                if (sharedMaterial != null)
                {
                    sharedMaterial.SetFloat(transparencyProperty, transparency);
                    sharedMaterial.SetFloat(xProperty, x);
                    sharedMaterial.SetFloat(yProperty, y);
                    sharedMaterial.SetFloat(zProperty, z);
                    sharedMaterial.SetFloat(spreadProperty, spread);

                }
            }
        }
    }
}
