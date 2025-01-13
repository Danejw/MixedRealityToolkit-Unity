using System.Collections;
using UnityEngine;


namespace ClearView
{
    /// <summary>
    /// This script manages the scaling of the model, increasing and decreasing it by a specified percentage.
    /// </summary>
    [RequireComponent(typeof(ModelManager))]
    public class ModelScaler : MonoBehaviour
    {
        [SerializeField] private ModelManager modelManager; // Reference to the ModelManager component
        [SerializeField] private Vector3 originalScale = new Vector3(.001f, 0.001f, .001f); // Hard-codes the original scale of the model
        [SerializeField] private float lerpDuration = 1.0f; // Duration for the lerp effect


        private Coroutine scaleCoroutine;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        private void OnEnable()
        {
            // Get the ModelManager component attached to the same GameObject
            if (!modelManager) modelManager = GetComponent<ModelManager>();

            // Store the original scale of the model now if needed
        }

        /// <summary>
        /// Scales the model by a specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage to scale the model by.</param>
        public void Scale(float percentage)
        {
            // Set the scale of the model by the specified percentage
            if (modelManager && modelManager.currentModel)
            {
                // Calculate the target scale based on the current scale and the percentage
                Vector3 currentScale = modelManager.currentModel.transform.localScale;
                Vector3 targetScale = currentScale * (1 + percentage);

                if (scaleCoroutine != null)
                {
                    StopCoroutine(scaleCoroutine);
                }
                scaleCoroutine = StartCoroutine(LerpScale(targetScale));
            }
            else
            {
                Logger.Log(Logger.Category.Warning, "A Model is not selected to scale.");
            }
        }

        /// <summary>
        /// Sets the scale of the model to the specified value.
        /// </summary>
        /// <param name="size">The new scale value to set for the model.</param>
        public void SetScaleTo(float size)
        {
            // Set the scale of the model by the specified percentage
            if (modelManager && modelManager.currentModel)
            {
                Vector3 targetScale = new Vector3(size, size, size);
                if (scaleCoroutine != null)
                {
                    StopCoroutine(scaleCoroutine);
                }
                scaleCoroutine = StartCoroutine(LerpScale(targetScale));
            }
            else
            {
                Logger.Log(Logger.Category.Warning, "A Model is not selected to scale.");
            }
        }

        /// <summary>
        /// Resets the scale of the model to its original value.
        /// </summary>
        public void ResetScale()
        {
            // Reset the scale of the model to its original value
            if (modelManager && modelManager.currentModel)
            {
                if (scaleCoroutine != null)
                {
                    StopCoroutine(scaleCoroutine);
                }
                scaleCoroutine = StartCoroutine(LerpScale(originalScale));
            }
        }

        /// <summary>
        /// Coroutine to smoothly lerp the scale of the model.
        /// </summary>
        /// <param name="targetScale">The target scale to lerp to.</param>
        /// <returns></returns>
        private IEnumerator LerpScale(Vector3 targetScale)
        {
            Vector3 initialScale = modelManager.currentModel.transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < lerpDuration)
            {
                modelManager.currentModel.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / lerpDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            modelManager.currentModel.transform.localScale = targetScale;
        }

        private void OnDisable()
        {
            ResetScale();
        }
    }
}
