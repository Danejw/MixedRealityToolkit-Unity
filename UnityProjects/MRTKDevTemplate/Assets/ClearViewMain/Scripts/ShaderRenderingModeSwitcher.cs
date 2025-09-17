using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ClearView
{
    public class ShaderRenderingModeSwitcher : MonoBehaviour
    {
        [Header("Target Renderers")]
        [SerializeField] private List<Renderer> renderers = new List<Renderer>();
        private List<Material> cachedMaterials = new List<Material>();


        // slider for alpha value
        [Header("Alpha Value")]
        [Range(0,1)]
        [SerializeField] public float alphaValue = 1.0f;

        // Ensures there's a renderer attached
        private void Awake()
        {
            renderers.AddRange(GetComponentsInChildren<Renderer>());

            foreach (var renderer in renderers)
            {
                if (renderer.material != null)
                {
                    cachedMaterials.Add(renderer.material);
                }
            }

            if (renderers.Count == 0)
            {
                Logger.Log(Logger.Category.Error, "No Renderer components found in the GameObject's children.");
            }
        }

        private void Update()
        {
            SetAlphaForAll(alphaValue);

            foreach (var renderer in renderers)
            {
                if (alphaValue > 0.9f)
                {
                    SetRenderingMode(renderer.material, "Opaque");
                }
                else
                {
                    SetRenderingMode(renderer.material, "Transparent");
                }
            }
        }

        /// <summary>
        /// Toggles the rendering mode of the attached material between Transparent and Opaque.
        /// </summary>
        public void ToggleRenderingMode()
        {
            if (renderers == null)
            {
                Logger.Log(Logger.Category.Error, "Renderer or Material is missing.");
                return;
            }

            foreach (var renderer in renderers)
            {
                if (renderer.material.GetFloat("_Mode") == 3f)
                {
                    SetRenderingMode(renderer.material, "Opaque");
                }
                else
                {
                    SetRenderingMode(renderer.material, "Transparent");
                }
            }
        }

        /// <summary>
        /// Sets the rendering mode of a material.
        /// </summary>
        /// <param name="material">The material to modify.</param>
        /// <param name="transparent">True for Transparent, False for Opaque.</param>
        private void SetRenderingMode(Material material, string mode)
        {
            switch (mode)
            {
                case "Opaque":
                    material.SetFloat("_Mode", 0); // Set mode to Opaque
                    material.SetOverrideTag("RenderType", "Opaque");

                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1); // Write to depth buffer

                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    break;

                case "Transparent":
                    material.SetFloat("_Mode", 3); // Set mode to Transparent
                    material.SetOverrideTag("RenderType", "Transparent");

                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0); // Disable depth writing

                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;

                default:
                    Debug.LogError("Unsupported rendering mode.");
                    return;
            }

#if UNITY_EDITOR
            // Update the material in the Unity Editor
            EditorUtility.SetDirty(material);
#endif
        }




        /// <summary>
        /// Sets the alpha value for all materials in child renderers.
        /// </summary>
        /// <param name="alpha">Alpha value to set.</param>
        private void SetAlphaForAll(float alpha)
        {
            foreach (var renderer in renderers)
            {
                if (renderer.material == null)
                {
                    Logger.Log(Logger.Category.Error, "A child Renderer has no material.");
                    continue;
                }

                Material material = renderer.material;

                // Modify alpha only if in transparent mode
                if (material.GetFloat("_Mode") == 3)
                {
                    material.SetFloat("_Alpha Fade", alpha);

                    var color = material.GetColor("_Color");
                    color.a = alpha;
                    material.SetColor("_Color", color);
                }
            }
        }




        public void SetAlpha(float alpha)
        {
            alphaValue = alpha;
        }
    }


}
