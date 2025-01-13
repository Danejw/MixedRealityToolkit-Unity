using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;


namespace ClearView
{
    public class ModelDetailsPanel : MonoBehaviour
    {
        private GameObject _model;
        public GameObject Model
        {
            get {  return _model; }
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

        private PhotonView photonView;


        private void Start()
        {
            if (!photonView) photonView = GetComponent<PhotonView>();

            rotationSlider.OnValueUpdated.AddListener(OnRotationSliderChanged);
            transparencySlider.OnValueUpdated.AddListener(OnTransparencySliderChanged);

            Close();
        }

        private void OnEnable()
        {
            if (!photonView) photonView = GetComponent<PhotonView>();

            Close();
        }

        private void OnTransparencySliderChanged(SliderEventData value)
        {
            //if (!photonView.IsMine) return;

            if (transparencyEditor) transparencyEditor.transparencyLevel = value.NewValue;

            Debug.Log("Transparency Slider Changed");
        }

        private void OnRotationSliderChanged(SliderEventData value)
        {
            //if (!photonView.IsMine) return;

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
        }

        public void Close()
        {
            //if (!photonView.IsMine) return;

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
            //if (!photonView.IsMine) return;

            layerToggles.SetToggleCollection(Model.transform);
            rotator.Setup(model);
            transparencyEditor.Setup(model);
        }

        public void SetModel(GameObject model)
        {
            Model = model;
        }

        // Inspector Button Helper
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
