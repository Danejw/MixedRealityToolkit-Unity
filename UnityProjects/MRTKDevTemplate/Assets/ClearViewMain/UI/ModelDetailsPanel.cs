using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace ClearView
{
    public class ModelDetailsPanel : MonoBehaviour
    {
        private GameObject _model;
        public GameObject Model
        {
            get { return _model; }
            private set
            {
                _model = value;
                SetUp(_model.transform);
            }
        }

        public enum DetailsState
        {
            Close,
            Open,
            Hidden
        }

        [Space(10)]
        [SerializeField] private DetailsState state = DetailsState.Hidden;

        [Space(10)]
        public GameObject openButton;
        public GameObject closeButton;
        public GameObject detailsParent;

        [Space(10)]
        public CustomToggleCollection layerToggles;

        [Space(10)]
        public Rotator rotator;
        public TransparencyEditor transparencyEditor;

        [Space(10)]
        public Slider rotationSlider;
        public Slider transparencySlider;

        [Header("Face Camera")]
        [SerializeField] private bool faceCameraWhenOpen = true;
        [SerializeField] private float retargetInterval = 0.15f;
        [SerializeField] private float minCameraMoveDistance = 0.03f;
        [SerializeField] private float minCameraRotateAngle = 3f;
        [SerializeField] private float rotationSmoothSpeed = 8f;
        [SerializeField] private bool flattenYAxis = true;

        private PhotonView photonView;

        private Transform cam;
        private Quaternion targetRotation;
        private Vector3 lastCameraPosition;
        private Quaternion lastCameraRotation;
        private float nextRetargetTime;

        private void Start()
        {
            if (!photonView) photonView = GetComponent<PhotonView>();

            rotationSlider.OnValueUpdated.AddListener(OnRotationSliderChanged);
            transparencySlider.OnValueUpdated.AddListener(OnTransparencySliderChanged);

            Close();

            StartCoroutine(SetupDetails());
        }

        private IEnumerator SetupDetails()
        {
            detailsParent?.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            detailsParent?.SetActive(false);
        }

        private void OnEnable()
        {
            if (!photonView) photonView = GetComponent<PhotonView>();
            Close();
        }

        private void Update()
        {
            if (!faceCameraWhenOpen) return;
            if (state != DetailsState.Open) return;
            if (detailsParent == null || !detailsParent.activeInHierarchy) return;

            if (cam == null && Camera.main != null)
            {
                cam = Camera.main.transform;
                lastCameraPosition = cam.position;
                lastCameraRotation = cam.rotation;
                ForceRetarget();
            }

            if (cam == null) return;

            if (Time.time >= nextRetargetTime)
            {
                bool movedEnough = Vector3.Distance(cam.position, lastCameraPosition) >= minCameraMoveDistance;
                bool rotatedEnough = Quaternion.Angle(cam.rotation, lastCameraRotation) >= minCameraRotateAngle;

                if (movedEnough || rotatedEnough)
                {
                    UpdateTargetRotation();
                    lastCameraPosition = cam.position;
                    lastCameraRotation = cam.rotation;
                }

                nextRetargetTime = Time.time + retargetInterval;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }

        private void ForceRetarget()
        {
            if (Camera.main == null) return;

            cam = Camera.main.transform;
            lastCameraPosition = cam.position;
            lastCameraRotation = cam.rotation;
            UpdateTargetRotation();
            nextRetargetTime = Time.time + retargetInterval;
        }

        private void UpdateTargetRotation()
        {
            if (cam == null) return;

            Vector3 toCamera = cam.position - transform.position;

            if (flattenYAxis)
            {
                toCamera.y = 0f;
            }

            if (toCamera.sqrMagnitude < 0.0001f) return;

            // Remove the minus if the panel faces backwards
            targetRotation = Quaternion.LookRotation(-toCamera.normalized, Vector3.up);
        }

        private void OnTransparencySliderChanged(SliderEventData value)
        {
            if (transparencyEditor) transparencyEditor.transparencyLevel = value.NewValue;
        }

        private void OnRotationSliderChanged(SliderEventData value)
        {
            if (rotator) rotator.SetRotationSpeed((int)value.NewValue);
        }

        public void Toggle(DetailsState state)
        {
            if (!photonView.IsMine) return;

            switch (state)
            {
                case DetailsState.Open:
                    Open();
                    break;
                case DetailsState.Close:
                    Close();
                    break;
                case DetailsState.Hidden:
                    Hide();
                    break;
            }
        }

        public void Open()
        {
            if (!photonView.IsMine) return;

            openButton.SetActive(false);
            closeButton.SetActive(true);
            detailsParent.SetActive(true);

            state = DetailsState.Open;
            ForceRetarget();
        }

        public void Close()
        {
            openButton.SetActive(true);
            closeButton.SetActive(false);
            detailsParent.SetActive(false);

            state = DetailsState.Close;
        }

        public void Hide()
        {
            if (!photonView.IsMine) return;

            openButton.SetActive(false);
            detailsParent.SetActive(false);

            state = DetailsState.Hidden;
        }

        public void SetUp(Transform model)
        {
            layerToggles.SetToggleCollection(Model.transform);
            rotator.Setup(model);
            transparencyEditor.Setup(model);
        }

        public void SetModel(GameObject model)
        {
            Model = model;
        }

        public void ToggleDetailsMenu()
        {
            if (!photonView.IsMine) return;

            switch (state)
            {
                case DetailsState.Close:
                    Open();
                    break;
                case DetailsState.Open:
                    Close();
                    break;
            }
        }
    }
}