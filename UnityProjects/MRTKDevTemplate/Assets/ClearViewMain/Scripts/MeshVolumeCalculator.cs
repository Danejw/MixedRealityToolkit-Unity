using UnityEngine;

namespace ClearView
{
    // Static class for calculating the volume of a 3D mesh
    public static class MeshVolumeCalculator
    {
        // Calculates the volume of the given mesh
        public static float CalculateMeshVolume(Mesh mesh)
        {
            // Check if the mesh is null and log an error if so
            if (mesh == null)
            {
                Debug.LogError("Mesh is null.");
                return 0f;
            }

            // Get the vertices and triangle indices from the mesh
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            float volume = 0f;

            // Iterate over each triangle in the mesh
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Get the vertices of the current triangle
                Vector3 p1 = vertices[triangles[i]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];

                // Accumulate the signed volume of the tetrahedron formed by the triangle and the origin
                volume += SignedVolumeOfTriangle(p1, p2, p3);
            }

            // Return the absolute value of the total volume
            return Mathf.Abs(volume);
        }

        // Calculates the signed volume of a tetrahedron formed by a triangle and the origin
        private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Use the scalar triple product to calculate the signed volume
            float v321 = p3.x * p2.y * p1.z;
            float v231 = p2.x * p3.y * p1.z;
            float v312 = p3.x * p1.y * p2.z;
            float v132 = p1.x * p3.y * p2.z;
            float v213 = p2.x * p1.y * p3.z;
            float v123 = p1.x * p2.y * p3.z;

            // Return the signed volume, scaled by 1/6
            return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }
    }
}