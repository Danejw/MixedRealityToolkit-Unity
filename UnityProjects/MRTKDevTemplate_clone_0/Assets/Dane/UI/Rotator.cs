using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ClearView
{
    public class Rotator : MonoBehaviour
    {
        public Transform modelToRotate;

        // Rotation speed variables
        public Vector3 rotationAxis = new Vector3(0, 1, 0); // Default rotation axis (Y-axis)

        [Range(-100, 100)]
        public float currentSpeed = 0;  // Current speed at which the object is rotating

        public void Setup(Transform model)
        {
            modelToRotate = model;
            SetRotationSpeed(0);
        }

        private void Update()
        {
            // Rotate the model around the specified axis, based on the current speed
            modelToRotate?.Rotate(rotationAxis * currentSpeed * Time.deltaTime);
        }

        // Function to dynamically set the rotation speed
        public void SetRotationSpeed(float speed)
        {
            currentSpeed = speed;
        }

        // Function to dynamically set the rotation axis
        public void SetRotationAxis(Vector3 axis)
        {
            rotationAxis = axis;
        }
    }
}
