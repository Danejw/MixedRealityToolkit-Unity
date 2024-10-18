using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;


namespace ClearView
{
    public class ModelDetailsPanel : MonoBehaviour
    {
        public GameObject model;

        public enum DetailsState
        {
            Close,
            Open
        }

        [Space(10)]
        [SerializeField] private bool isOpen = false;

        [Space(10)]
        public GameObject openButton;
        public GameObject detailsParent;

        [Space(10)]
        public CustomToggleCollection layerToggles;

        [Space(10)]
        public Rotator rotator;
        public TransparencyEditor transparencyEditor;

        [Space(10)]
        public Slider rotationSlider;
        public Slider transparencySlider;

        private void Start()
        {
            rotationSlider.OnValueUpdated.AddListener(OnRotationSliderChanged);
            transparencySlider.OnValueUpdated.AddListener(OnTransparencySliderChanged);
        }

        private void OnEnable()
        {
            Close();
        }

        private void OnTransparencySliderChanged(SliderEventData value)
        {
            if (transparencyEditor) transparencyEditor.transparencyLevel = value.NewValue;
        }

        private void OnRotationSliderChanged(SliderEventData value)
        {
            if (rotator) rotator.SetRotationSpeed((int)value.NewValue);
        }

        public void Toggle(bool open)
        {
            switch (open)
            {
                case true:
                    Open();
                    break;
                case false:
                    Close();
                    break;
            }
        }

        public void ToggleDetailsMenu()
        {
            switch (isOpen)
            {
                case false:
                    Open();
                    break;
                case true:
                    Close();
                    break;
            }
        }

        private void Open()
        {
            openButton.SetActive(false);
            detailsParent.SetActive(true);

            isOpen = true;
        }

        private void Close()
        {
            openButton.SetActive(true);
            detailsParent.SetActive(false);

            isOpen = false;
        }

        public void PopulateToggles()
        {
            layerToggles.SetToggleCollection(model.transform);
        }
    }
}
