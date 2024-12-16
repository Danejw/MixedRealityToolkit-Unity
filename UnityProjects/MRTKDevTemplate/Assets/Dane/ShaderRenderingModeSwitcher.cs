using System.Collections.Generic;
using UnityEngine;

namespace ClearView
{
    public class ShaderRenderingModeSwitcher : MonoBehaviour
    {
        [Header("Target Renderers")]
        [SerializeField] private List<Renderer> renderers = new List<Renderer>();


        // slider for alpha value
        [Header("Alpha Value")]
        [Range(0,1)]
        [SerializeField] public float alphaValue = 1.0f;

        // Ensures there's a renderer attached
        private void Awake()
        {
            // Gather all Renderer components in children
            renderers.AddRange(GetComponentsInChildren<Renderer>());

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
                    SetRenderingMode(renderer.material, false);
                }
                else
                {
                    SetRenderingMode(renderer.material, true);
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
                if (renderer.material.GetFloat("_Rendering Mode") == 1.0f)
                {
                    SetRenderingMode(renderer.material, false);
                }
                else
                {
                    SetRenderingMode(renderer.material, true);
                }
            }
        }

        /// <summary>
        /// Sets the rendering mode of a material.
        /// </summary>
        /// <param name="material">The material to modify.</param>
        /// <param name="transparent">True for Transparent, False for Opaque.</param>
        private void SetRenderingMode(Material material, bool transparent)
        {
            if (transparent)
            {
                material.SetFloat("_Rendering Mode", 1.0f); // Transparent
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
            }
            else
            {
                material.SetFloat("_Rendering Mode", 0.0f); // Opaque
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
            }
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
                material.SetFloat("_Alpha Fade", alpha);

                var color = material.GetColor("_Color");
                color.a = alpha;
                material.SetColor("_Color", color);
            }
        }



        public void SetAlpha(float alpha)
        {
            alphaValue = alpha;
        }
    }


}
