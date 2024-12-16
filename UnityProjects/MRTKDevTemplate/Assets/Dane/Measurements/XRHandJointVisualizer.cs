using System.Collections.Generic;
using UnityEngine;

namespace ClearView
{
    public class XRHandJointVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField, Tooltip("The prefab to represent each joint in the scene.")]
        private GameObject jointPrefab;

        [SerializeField, Tooltip("Color of the visualized joints.")]
        private Color jointColor = Color.green;

        private Dictionary<string, GameObject> jointVisuals = new Dictionary<string, GameObject>();

        private void Start()
        {
            if (XRHandJointManager.Instance == null)
            {
                Debug.LogError("XRHandJointManager is not initialized. Ensure it exists in the scene.");
                return;
            }

            InitializeJointVisuals();
        }

        public void InitializeJointVisuals()
        {
            foreach (var joint in XRHandJointManager.Instance.GetJoints())
            {
                // Instantiate a visual object for each joint
                GameObject visual = Instantiate(jointPrefab);
                visual.name = joint.Key + "_Visual";
                visual.transform.SetParent(transform);

                // Set the color of the visual
                var renderer = visual.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = jointColor;
                }

                jointVisuals[joint.Key] = visual;
            }
        }

        private void Update()
        {
            foreach (var jointName in jointVisuals.Keys)
            {
                Transform jointTransform = XRHandJointManager.Instance.GetJointTransform(jointName);
                if (jointTransform != null)
                {
                    jointVisuals[jointName].transform.position = jointTransform.position;
                    jointVisuals[jointName].transform.rotation = jointTransform.rotation;
                }
            }
        }
    }
}
