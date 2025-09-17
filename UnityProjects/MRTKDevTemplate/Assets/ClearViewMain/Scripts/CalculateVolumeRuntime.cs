using UnityEngine;

namespace ClearView
{
    public class CalculateVolumeRuntime : MonoBehaviour
    {
        [Tooltip("The mesh to calculate the volume of")]
        public MeshFilter meshFilter;

        [Tooltip("Volume in m³")]
        public float volume;

        private void Start()
        {
            volume = MeshVolumeCalculator.CalculateMeshVolume(meshFilter.mesh);
            Debug.Log("Volume (m³): " + volume);
        }

        public void CalcMesh()
        {
            volume = MeshVolumeCalculator.CalculateMeshVolume(meshFilter.mesh);
            Debug.Log("Volume (m³): " + volume);
        }
    }
}