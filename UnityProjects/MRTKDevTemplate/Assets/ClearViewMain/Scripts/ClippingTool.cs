
using UnityEngine;


namespace ClearView
{
    public class ClippingTool : MonoBehaviour
    {
        [SerializeField] private ClippingMode _clippingMode;
        public ClippingMode clippingMode
        {
            get { return _clippingMode; }
            set
            {
                _clippingMode = value;
                ChangeClippingMode(value);
            }
        }

        public enum ClippingMode
        {
            None,
            Plane,
            Cube,
            Sphere
        }

        [Space]
        public GameObject clippingPlane;
        public GameObject clippingCube;
        public GameObject clippingSphere;

        public void ChangeClippingMode(ClippingMode mode)
        {
            switch (mode)
            {
                case ClippingMode.None:
                    clippingCube.SetActive(false);
                    clippingPlane.SetActive(false);
                    clippingSphere.SetActive(false);
                    break;
                case ClippingMode.Plane:
                    clippingCube.SetActive(false);
                    clippingPlane.SetActive(true);
                    clippingSphere.SetActive(false);
                    break;
                case ClippingMode.Cube:
                    clippingCube.SetActive(true);
                    clippingPlane.SetActive(false);
                    clippingSphere.SetActive(false);
                    break;
                case ClippingMode.Sphere:
                    clippingCube.SetActive(false);
                    clippingPlane.SetActive(false);
                    clippingSphere.SetActive(true);
                    break;
            }
        }

        public void SetClippingModeToPlane() => clippingMode = ClippingMode.Plane;
        public void SetClippingModeToCube() => clippingMode = ClippingMode.Cube;
        public void SetClippingModeToSphere() => clippingMode = ClippingMode.Sphere;
        public void SetClippingModeToNone() => clippingMode = ClippingMode.None;


        private void Start()
        {
            clippingMode = ClippingMode.None;
        }
    }
}
